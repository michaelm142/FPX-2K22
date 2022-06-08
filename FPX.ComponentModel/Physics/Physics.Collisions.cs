using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace FPX
{
    public partial class Physics
    {

        private void DetectCollisions(ref List<Collision> collisions)
        {
            for (int i = 0; i < colliders.Count - 1; i++)
            {
                Collider a = colliders[i];
                for (int ii = i + 1; ii < colliders.Count; ii++)
                {
                    Collider b = colliders[ii];
                    var collision = Collide(a, b);
                    if (collision != null)
                        collisions.Add(collision);
                }
            }
        }

        private Collision Collide(Collider a, Collider b)
        {
            if (previousPositions.Count == 0)
                return null;

            if (a is SphereCollider && b is SphereCollider)
            {
                Collision c = SphereToSphere(a as SphereCollider, b as SphereCollider);
                SendColisionMessages(a, b, c != null);
                return c;
            }
            if (a is BoxCollider && b is BoxCollider)
            {
                Collision c = BoxToBox(a as BoxCollider, b as BoxCollider);
                if (c != null)
                    Debug.Log("PsudoDistance: {0}", c.Psudodistance);
                SendColisionMessages(a, b, c != null);
                return c;
            }
            if (a is BoxCollider && b is SphereCollider)
            {
                Collision c = BoxToSphere(a as BoxCollider, b as SphereCollider);
                SendColisionMessages(a, b, c != null);
                return c;
            }
            if (a is SphereCollider && b is BoxCollider)
            {
                Collision c = SphereToBox(a as SphereCollider, b as BoxCollider);
                SendColisionMessages(a, b, c != null);
                return c;
            }

            return null;
        }

        private void ResolveCollision(Collision collision)
        {
            foreach (var c in collision.colliders)
                if (c.GetComponent<Rigidbody>() == null)
                    return;

            Collider a = collision[0];
            Collider b = collision[1];

            Vector3 contactNormalA = Vector3.Zero;
            Vector3 contactNormalB = Vector3.Zero;

            Vector3 closestPointA = a.ClosestPoint(b.position, out contactNormalA);
            Vector3 closestPointB = b.ClosestPoint(closestPointA, out contactNormalB);
            closestPointA = a.ClosestPoint(closestPointB);

            Vector3 L_a = closestPointB - closestPointA;
            Vector3 L_b = closestPointA - closestPointB;

            var bodyA = a.GetComponent<Rigidbody>();
            var bodyB = b.GetComponent<Rigidbody>();

            if (bodyA == null || bodyB == null)
                return;

            for (int i = 0; i < MaxPhysIterations; i++)
            {
                float psudoDistance = Psudodistance(a, b);
                float itr = 1.0f / (i + 1.0f);
                Debug.Log("Iteration {0}, Psudodistance: {1}, itr: {2}", i, psudoDistance, itr);
                bodyA.position += bodyA.velocity.Normalized() * (psudoDistance * itr);
                bodyB.position += bodyB.velocity.Normalized() * (psudoDistance * itr);

                // Vector3 bodyA_w = bodyA.angularVelocity.Normalized();
                // Vector3 bodyB_w = bodyB.angularVelocity.Normalized();

                // bodyA.rotation *= LinearAlgebraUtil.QuaternionFromEuler(-bodyA_w * -psudoDistance * itr);// Quaternion.CreateFromYawPitchRoll(-bodyA_w.Y * -psudoDistance * itr, -bodyA_w.X * -psudoDistance * itr, -bodyA_w.Z * -psudoDistance * itr);
                // bodyB.rotation *= LinearAlgebraUtil.QuaternionFromEuler(-bodyB_w * -psudoDistance * itr);// Quaternion.CreateFromYawPitchRoll(-bodyB_w.Y * -psudoDistance * itr, -bodyB_w.X * -psudoDistance * itr, -bodyB_w.Z * -psudoDistance * itr);

                //bodyA.velocity += bodyA.velocity * (psudoDistance * itr);
                //bodyB.velocity -= bodyB.velocity * (-psudoDistance * itr);
            }

            closestPointA = a.ClosestPoint(b.position);
            closestPointB = b.ClosestPoint(a.position);
            closestPointA = a.ClosestPoint(closestPointB);

            L_a = closestPointA - a.position;
            L_b = closestPointB - b.position;

            var velocityA = Vector3.Reflect(bodyB.velocity, collision.ContactNormal);
            var velocityB = Vector3.Reflect(bodyA.velocity, -collision.ContactNormal);
            Debug.Log("Contact Normal: {0}", collision.ContactNormal);

            bodyA.velocity = velocityA;
            bodyB.velocity = velocityB;

            Debug.Log("Velocity A: {0} Velocity B: {1}", velocityA, velocityB);

            var accelerationA = Vector3.Reflect(bodyB.acceleration, collision.ContactNormal);
            var accelerationB = Vector3.Reflect(bodyA.acceleration, -collision.ContactNormal);

            bodyA.acceleration = accelerationA;
            bodyB.acceleration = accelerationB;

            Matrix contactTensorA = Matrix.Identity;
            Matrix velocityTensorA = Matrix.Identity;

            contactTensorA.Forward = L_a;
            contactTensorA.Up = collision.ContactNormal;
            contactTensorA.Right = Vector3.Cross(contactTensorA.Forward, contactTensorA.Up);

            velocityTensorA.Forward = velocityA;
            velocityTensorA.Right = Vector3.Cross(Vector3.Up, velocityA.Normalized());
            velocityTensorA.Up = Vector3.Cross(velocityTensorA.Forward, velocityTensorA.Right).Normalized();

            float yawA = Vector3.Dot(contactTensorA.Right, velocityTensorA.Forward);
            float pitchA = Vector3.Dot(contactTensorA.Up, velocityTensorA.Forward);
            float rollA = Vector3.Dot(contactTensorA.Up, velocityTensorA.Right);

            if (float.IsNaN(yawA)) yawA = 0.0f;
            if (float.IsNaN(pitchA)) pitchA = 0.0f;
            if (float.IsNaN(rollA)) rollA = 0.0f;

            bodyA.angularVelocity += new Vector3(pitchA, yawA, rollA);
            Debug.Log("{0} Yaw: {1} Pitch: {2} Roll: {3}", bodyA.gameObject.Name, yawA, pitchA, rollA);


            Matrix contactTensorB = Matrix.Identity;
            Matrix velocityTensorB = Matrix.Identity;

            contactTensorB.Forward = L_b;
            contactTensorB.Up = -collision.ContactNormal;
            contactTensorB.Right = Vector3.Cross(contactTensorB.Forward, contactTensorB.Up).Normalized();

            velocityTensorB.Forward = velocityB;
            velocityTensorB.Right = Vector3.Cross(Vector3.Up, velocityB.Normalized());
            velocityTensorB.Up = Vector3.Cross(velocityTensorB.Right, velocityTensorB.Forward);

            float yawB = Vector3.Dot(contactTensorB.Right, velocityTensorB.Forward);
            float pitchB = Vector3.Dot(contactTensorB.Up, velocityTensorB.Forward);
            float rollB = Vector3.Dot(contactTensorB.Up, velocityTensorB.Right);

            if (float.IsNaN(yawB)) yawB = 0.0f;
            if (float.IsNaN(pitchB)) pitchB = 0.0f;
            if (float.IsNaN(rollB)) rollB = 0.0f;

            bodyB.angularVelocity += new Vector3(pitchB, yawB, rollB);
            Debug.Log("{0} Yaw: {1} Pitch: {2} Roll: {3}", bodyB.gameObject.Name, yawB, pitchB, rollB);
        }

        #region Collision Detection

        private float Psudodistance(Collider colliderA, Collider colliderB)
        {
            if (colliderA is BoxCollider && colliderB is BoxCollider)
                return Psudodistance(colliderA as BoxCollider, colliderB as BoxCollider);
            else
                return Psudodistance(colliderA as SphereCollider, colliderB as SphereCollider);
        }

        private float Psudodistance(SphereCollider a, SphereCollider b)
        {
            Vector3 L = b.position - a.position;
            float lengthSquared = L.LengthSquared();
            float sumRadi = a.radius + b.radius;
            return lengthSquared / (sumRadi * sumRadi) - 1.0f;
        }

        private float Psudodistance(BoxCollider a, BoxCollider b)
        {
            Vector3 L = b.Location - a.Location;
            float lengthSquared = L.LengthSquared();
            float rSum = a.size.Length() + b.size.Length();

            return lengthSquared / (rSum * rSum) - 1.0f;

        }

        private float Psudodistance(float p0, float p1, float q0, float q1, float u, float v, float t, out float asub2, out float asub1, out float asub0)
        {
            asub2 = (u - v) * (u - v);
            asub1 = p0 * u - p0 * v + p1 * p1 * u - p1 * v;
            asub0 = (p0 - q1) * (p1 - q0);
            float F = asub2 * (t * t) + asub1 * t + asub0;
            return F;
            //return (u - v) * (u - v) * (t * t) + 2 * (u - v) * ((p0 - p1) / 2.0F - (q1 + q0) / 2.0F) * t + (p0 - q1) * (p1 - q0);
        }

        private ContactType FindContactType(Collider a, Collider b, out float psudoDistance)
        {
            psudoDistance = Psudodistance(a, b);
            if (psudoDistance < 0.0f)
                return ContactType.Overlapping;
            else if (LinearAlgebraUtil.isEpsilon(psudoDistance))
                return ContactType.Touching;

            return ContactType.Seperated;
        }

        private Collision SphereToSphere(SphereCollider a, SphereCollider b)
        {
            float distance = Vector3.Distance(a.Location, b.Location);
            if (distance > a.radius + b.radius)
                return null;

            Collision collision = new Collision(a, b);
            float penetratingRadius = Vector3.Distance(a.position, b.position) - (a.radius + b.radius);
            Vector3 L = b.position - a.position;
            collision.PenetrationDistance = penetratingRadius;
            L.Normalize();
            collision.L = L;

            Vector3 Ra = L * a.radius;
            Vector3 Rb = L * b.radius;

            Vector3 nPrime = Vector3.Cross(Vector3.Up, L);
            collision.ContactNormal = Vector3.Cross(L, nPrime);

            collision.Psudodistance = Psudodistance(a, b);

            return collision;
        }

        private Collision SphereToBox(SphereCollider a, BoxCollider b)
        {
            if (MathHelper.Distance(a.Psudosize.Length(), b.Psudosize.Length()) > a.Psudosize.Length() + b.Psudosize.Length())
                return null;

            var closestPoint = b.ClosestPoint(a.Location);

            Vector3 L = closestPoint - a.Location;
            if (Vector3.Dot(L, L) <= a.radius * a.radius)
            {
                Collision c = new Collision(a, b);
                c.L = L;
                c.ContactNormal = L.Normalized();
                c.Psudodistance = Psudodistance(a, b);
                return c;
            }

            return null;
        }

        private Collision BoxToSphere(BoxCollider a, SphereCollider b)
        {
            return SphereToBox(b, a);
        }

        private Collision BoxToBox(BoxCollider a, BoxCollider b)
        {
            if (MathHelper.Distance(a.Psudosize.Length(), b.Psudosize.Length()) > a.Psudosize.Length() + b.Psudosize.Length())
                return null;

            float ra, rb;
            Matrix R = Matrix.Identity,
                AbsR = Matrix.Identity;

            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    R = LinearAlgebraUtil.SetRowColumn(R, i, j, Vector3.Dot(a.transform.worldPose.GetRow(i).ToVector3(), b.transform.worldPose.GetRow(j).ToVector3()));

            Vector3 t = b.Location - a.Location;
            t = new Vector3(Vector3.Dot(t, a.transform.worldPose.GetRow(0).ToVector3()),
                Vector3.Dot(t, a.transform.worldPose.GetRow(1).ToVector3()),
                Vector3.Dot(t, a.transform.worldPose.GetRow(2).ToVector3()));

            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    AbsR = LinearAlgebraUtil.SetRowColumn(AbsR, i, j, Math.Abs(R.GetRowColumn(i, j)) + float.Epsilon);

            // test axis L = A0, L = A1, L = A2
            for (int i = 0; i < 3; i++)
            {
                ra = LinearAlgebraUtil.GetVectorIndex(a.size, i);
                rb = LinearAlgebraUtil.GetVectorIndex(b.size, 0) * AbsR.GetRowColumn(i, 0) +
                    LinearAlgebraUtil.GetVectorIndex(b.size, 1) * AbsR.GetRowColumn(i, 1) +
                    LinearAlgebraUtil.GetVectorIndex(b.size, 2) * AbsR.GetRowColumn(i, 2);
                if (Math.Abs(LinearAlgebraUtil.GetVectorIndex(t, i)) > ra + rb)
                    return null;
            }

            // test axis L = B0, L = B1, L = B2
            for (int i = 0; i < 3; i++)
            {
                ra = a.size.GetIndex(0) * AbsR.GetRowColumn(0, i) + a.size.GetIndex(1) * AbsR.GetRowColumn(1, i) + a.size.GetIndex(2) * AbsR.GetRowColumn(2, i);
                rb = b.size.GetIndex(i);

                if ((Math.Abs(t.GetIndex(0) * R.GetRowColumn(0, i)) + t.GetIndex(1) * R.GetRowColumn(1, i) + t.GetIndex(2) * R.GetRowColumn(2, i)) > ra + rb)
                    return null;
            }

            // Test axis L = A0 x B0
            ra = a.size.GetIndex(1) * AbsR.GetRowColumn(2, 0) + a.size.GetIndex(2) * AbsR.GetRowColumn(1, 0);
            rb = b.size.GetIndex(1) * AbsR.GetRowColumn(0, 2) + b.size.GetIndex(2) * AbsR.GetRowColumn(0, 1);
            if (Math.Abs(t.GetIndex(2) * R.GetRowColumn(1, 0) - t.GetIndex(1) * R.GetRowColumn(2, 0)) > ra + rb)
                return null;

            // Test axis L = A0 x B1
            ra = a.size.GetIndex(1) * AbsR.GetRowColumn(2, 1) + a.size.GetIndex(2) * AbsR.GetRowColumn(1, 1);
            rb = b.size.GetIndex(0) * AbsR.GetRowColumn(0, 2) + b.size.GetIndex(2) * AbsR.GetRowColumn(0, 0);
            if (Math.Abs(t.GetIndex(2) * R.GetRowColumn(1, 1) - t.GetIndex(1) * R.GetRowColumn(2, 1)) > ra + rb)
                return null;

            // Test axis L = A0 x B2
            ra = a.size.GetIndex(1) * AbsR.GetRowColumn(2, 2) + a.size.GetIndex(2) * AbsR.GetRowColumn(1, 2);
            rb = b.size.GetIndex(0) * AbsR.GetRowColumn(0, 1) + a.size.GetIndex(1) * AbsR.GetRowColumn(0, 0);
            if (Math.Abs(t.GetIndex(2) * R.GetRowColumn(1, 2) - t.GetIndex(1) * R.GetRowColumn(2, 2)) > ra + rb)
                return null;

            // Test axis L = A1 x B0
            ra = a.size.GetIndex(0) * AbsR.GetRowColumn(2, 0) + a.size.GetIndex(2) * AbsR.GetRowColumn(0, 0);
            rb = b.size.GetIndex(1) * AbsR.GetRowColumn(1, 2) + a.size.GetIndex(2) * AbsR.GetRowColumn(1, 1);
            if (Math.Abs(t.GetIndex(0) * R.GetRowColumn(2, 0) - t.GetIndex(2) * R.GetRowColumn(0, 0)) > ra + rb)
                return null;

            // Test axis L = A1 x B1
            ra = a.size.GetIndex(0) * AbsR.GetRowColumn(2, 1) + a.size.GetIndex(2) * AbsR.GetRowColumn(0, 1);
            rb = b.size.GetIndex(0) * AbsR.GetRowColumn(1, 2) + b.size.GetIndex(2) * AbsR.GetRowColumn(1, 0);
            if (Math.Abs(t.GetIndex(0) * R.GetRowColumn(2, 1) - t.GetIndex(2) * R.GetRowColumn(0, 1)) > ra + rb)
                return null;

            // Test axis L = A1 x B2
            ra = a.size.GetIndex(0) * AbsR.GetRowColumn(2, 2) + a.size.GetIndex(2) * AbsR.GetRowColumn(0, 2);
            rb = b.size.GetIndex(0) * AbsR.GetRowColumn(1, 1) + b.size.GetIndex(1) * AbsR.GetRowColumn(1, 0);
            if (Math.Abs(t.GetIndex(0) * R.GetRowColumn(2, 2) - t.GetIndex(2) * R.GetRowColumn(0, 2)) > ra + rb)
                return null;

            // Test axis L = A2 x B0
            ra = a.size.GetIndex(0) * AbsR.GetRowColumn(1, 0) + a.size.GetIndex(1) * AbsR.GetRowColumn(0, 0);
            rb = b.size.GetIndex(1) * AbsR.GetRowColumn(2, 2) + b.size.GetIndex(2) * AbsR.GetRowColumn(2, 1);
            if (Math.Abs(t.GetIndex(1) * R.GetRowColumn(0, 0) - t.GetIndex(0) * R.GetRowColumn(1, 0)) > ra + rb)
                return null;

            // Test axis L = A2 x B1
            ra = a.size.GetIndex(0) * AbsR.GetRowColumn(1, 1) + a.size.GetIndex(1) * AbsR.GetRowColumn(0, 1);
            rb = b.size.GetIndex(0) * AbsR.GetRowColumn(2, 2) + b.size.GetIndex(2) * AbsR.GetRowColumn(2, 0);
            if (Math.Abs(t.GetIndex(1) * R.GetRowColumn(0, 1) - t.GetIndex(0) * R.GetRowColumn(1, 1)) > ra + rb)
                return null;

            // Test axis L = A2 x B2
            ra = a.size.GetIndex(0) * AbsR.GetRowColumn(1, 2) + a.size.GetIndex(1) * AbsR.GetRowColumn(0, 2);
            rb = b.size.GetIndex(0) * AbsR.GetRowColumn(2, 1) + b.size.GetIndex(1) * AbsR.GetRowColumn(2, 0);
            if (Math.Abs(t.GetIndex(1) * R.GetRowColumn(0, 2) - t.GetIndex(0) * R.GetRowColumn(1, 2)) > ra + rb)
                return null;

            Collision c = new Collision(a, b);
            Vector3 normalA = Vector3.Zero;
            Vector3 normalB = Vector3.Zero;

            a.ClosestPoint(b.position, out normalA);
            b.ClosestPoint(a.position, out normalB);

            c.ContactNormal = Vector3.Cross(normalA, normalB);
            if (a.GetComponent<Rigidbody>() != null && b.GetComponent<Rigidbody>() != null)
                c.Psudodistance = Psudodistance(a, b);

            return c;
        }


        #endregion
    }

    internal enum ContactType
    {
        Overlapping,
        Touching,
        Seperated,
    }
}
