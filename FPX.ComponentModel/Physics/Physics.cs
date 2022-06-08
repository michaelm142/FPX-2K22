using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using FPX.ComponentModel;

namespace FPX
{
    public partial class Physics : IGameComponent, IUpdateable
    {
        public bool Enabled { get; set; } = true;

        public int UpdateOrder { get; set; } = 100;

        public event EventHandler<EventArgs> EnabledChanged;
        public event EventHandler<EventArgs> UpdateOrderChanged;

        private Dictionary<Collider, Vector3> previousPositions = new Dictionary<Collider, Vector3>();

        IEnumerable<GameObject> colliderObjects
        {
            get { return GameCore.currentLevel.Objects.ToList().FindAll(o => o.GetComponent<Collider>() != null); }
        }

        IEnumerable<GameObject> rigidbodyObjects
        {
            get { return GameCore.currentLevel.Objects.ToList().FindAll(o => o.GetComponent<Rigidbody>() != null); }
        }

        IEnumerable<Collider> colliderEnumerable
        {
            get
            {
                foreach (var gameObject in colliderObjects)
                    yield return gameObject.GetComponent<Collider>();
            }
        }

        IEnumerable<Rigidbody> Rigidbodies
        {
            get
            {
                foreach (var gameObject in rigidbodyObjects)
                    yield return gameObject.GetComponent<Rigidbody>();
            }
        }

        List<Collider> colliders
        {
            get { return colliderEnumerable.ToList(); }
        }

        List<Collision> activeCollisions = new List<Collision>();

        public int MaxPhysIterations { get; private set; }

        public void Initialize()
        {
            MaxPhysIterations = Settings.GetSetting<int>("Physics/MaxPhysIterations");
            Debug.Log("Maximum number of collision iterations is {0}", MaxPhysIterations);
        }

        public void Update(GameTime gameTime)
        {
            if (GameCore.currentLevel == null)
                return;
            //Debug.Log("==================<Begin Physics Frame>==================");
            // detect and cull collisions
            List<Collision> collisions = new List<Collision>();
            DetectCollisions(ref collisions);
            activeCollisions = collisions;

            // resolve collisions and update physics state
            collisions.ForEach(c => ResolveCollision(c));

            // Update previous positions
            foreach (var collider in colliders)
            {
                if (previousPositions.ContainsKey(collider))
                    previousPositions[collider] = collider.Location;
                else
                    previousPositions.Add(collider, collider.Location);
            }

            // move objects
            foreach (var rBody in Rigidbodies)
                UpdateRigidbody(rBody);
            //Debug.Log("==================<End Physics Frame>==================");
        }

        private void UpdateRigidbody(Rigidbody rBody)
        {
            if (rBody.isKinematic || (rBody.acceleration.Length() == 0.0f && rBody.velocity.Length() == 0.0f))
                return;


            rBody.acceleration -= rBody.acceleration * rBody.drag * Time.deltaTime;
            rBody.torque -= rBody.torque * rBody.angularDrag * Time.deltaTime;

            rBody.velocity += rBody.acceleration * Time.deltaTime;
            rBody.velocity -= rBody.velocity * rBody.drag * Time.deltaTime;
            rBody.transform.position += rBody.velocity * Time.deltaTime;
            rBody.angularVelocity += rBody.torque * Time.deltaTime;
            rBody.transform.rotation *= Quaternion.CreateFromYawPitchRoll(
                rBody.angularVelocity.Y * Time.deltaTime,
                rBody.angularVelocity.Z * Time.deltaTime,
                rBody.angularVelocity.X * Time.deltaTime);

            if (LinearAlgebraUtil.isEpsilon(rBody.acceleration))
                rBody.acceleration = Vector3.Zero;
            if (LinearAlgebraUtil.isEpsilon(rBody.velocity))
                rBody.velocity = Vector3.Zero;
        }

        private void SendColisionMessages(Collider a, Collider b, bool value)
        {
            if (value)
            {
                if (activeCollisions.Find(activeCollision => activeCollision.colliders.Contains(a) && activeCollision.colliders.Contains(b)) == null)
                {
                    activeCollisions.Add(new Collision(a, b));
                    a.gameObject.BroadcastMessage("OnCollisionEnter", b);
                    b.gameObject.BroadcastMessage("OnCollisionEnter", a);
                }
                else
                {
                    a.gameObject.BroadcastMessage("OnCollision", b);
                    b.gameObject.BroadcastMessage("OnCollision", a);
                }
            }
            else if (activeCollisions.Find(activeCollision => activeCollision.colliders.Contains(a) && activeCollision.colliders.Contains(b)) != null)
            {
                a.gameObject.BroadcastMessage("OnCollisionExit", b);
                b.gameObject.BroadcastMessage("OnCollisionExit", a);
                activeCollisions.Remove(activeCollisions.Find(activeCollision => activeCollision.colliders.Contains(a) || activeCollision.colliders.Contains(b)));
            }
        }

        public static bool IntersectRaySphere(Ray ray, SphereCollider sphere, out float length, out Vector3 point)
        {
            Vector3 L = ray.Position - sphere.Location;
            float b = Vector3.Dot(L, ray.Direction);
            float c = Vector3.Dot(L, L) - sphere.radius * sphere.radius;

            if (c > 0.0f && b > 0.0f)
            {
                length = 0.0f;
                point = Vector3.Zero;
                return false;
            }

            float discriminant = b * b - c;
            if (discriminant < 0.0f)
            {
                length = 0.0f;
                point = Vector3.Zero;
                return false;
            }

            length = -b - (float)Math.Sqrt(discriminant);
            if (length < 0.0f)
                length = 0.0f;

            point = ray.Position + ray.Direction * length;
            return true;
        }

        public static bool IntersectRayBox(Ray ray, BoxCollider box, out float length, out Vector3 point)
        {
            float tMin = 0.0f;
            float tMax = float.PositiveInfinity;

            Matrix inv = Matrix.Invert(Matrix.CreateFromQuaternion(box.rotation) * Matrix.CreateTranslation(box.position));
            Vector3 localizedRayPosition = Vector3.Transform(ray.Position, inv);
            Vector3 localizedRayDirection = Vector3.TransformNormal(ray.Direction, inv);

            Ray localizedRay = new Ray(localizedRayPosition, localizedRayDirection);

            Vector3 max = box.size;
            Vector3 min = -box.size;

            for (int i = 0; i < 3; i++)
            {
                if (LinearAlgebraUtil.isEpsilon(Math.Abs(localizedRay.Direction.GetIndex(i))))
                {
                    if (localizedRay.Position.GetIndex(i) < min.GetIndex(i) || localizedRay.Position.GetIndex(i) > max.GetIndex(i))
                    {
                        length = 0.0f;
                        point = Vector3.Zero;
                        return false;
                    }
                }

                float ood = 1.0f / localizedRay.Direction.GetIndex(i);
                float t1 = (min.GetIndex(i) - localizedRay.Position.GetIndex(i)) * ood;
                float t2 = (max.GetIndex(i) - localizedRay.Position.GetIndex(i)) * ood;

                if (t1 > t2)
                {
                    float w = t1;
                    t1 = t2;
                    t2 = w;
                }

                tMin = Math.Max(tMin, t1);
                tMax = Math.Min(tMax, t2);

                if (tMin > tMax)
                {
                    length = 0.0f;
                    point = Vector3.Zero;
                    return false;
                }
            }

            point = ray.Position + ray.Direction * tMin;
            length = tMin;

            return true;
        }
    }
}
