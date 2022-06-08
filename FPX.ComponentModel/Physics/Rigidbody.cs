using System.Xml;
using Microsoft.Xna.Framework;
using FPX.ComponentModel;

namespace FPX
{
    public class Rigidbody : Component
    {
        public Vector3 velocity = Vector3.Zero;
        public Vector3 acceleration = Vector3.Zero;
        public Vector3 torque = Vector3.Zero;
        public Vector3 angularVelocity = Vector3.Zero;
        private Vector3 startPosition;
        private Vector3 startRotation;

        public float mass = 1.0F;
        public float drag = 0.0F;
        public float angularDrag = 0.0f;

        public bool isKinematic { get; set; }

        public Collider collider
        {
            get { return GetComponent<Collider>(); }
        }

        public void Start()
        {
            startPosition = transform.position;
            startRotation = transform.rotation.GetEulerAngles();
        }

        public void Update(GameTime gameTime)
        {
            if (isKinematic)
            {
                transform.position = startPosition;
                transform.rotation.SetEulerAngles(startRotation);
            }
        //    if (isKinematic || (acceleration.Length() == 0.0f && velocity.Length() == 0.0f))
        //        return;

        //    acceleration -= velocity * drag * (float)gameTime.ElapsedGameTime.TotalSeconds;
        //    torque -= angularVelocity * angularDrag * (float)gameTime.ElapsedGameTime.TotalSeconds;

        //    velocity += acceleration * (float)gameTime.ElapsedGameTime.TotalSeconds;
        //    transform.position += velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;
        //    angularVelocity += torque * (float)gameTime.ElapsedGameTime.TotalSeconds;
        //    transform.rotation *= Quaternion.CreateFromYawPitchRoll(angularVelocity.Y, angularVelocity.X, angularVelocity.Z);

        //    if (LinearAlgebraUtil.isEpsilon(acceleration))
        //        acceleration = Vector3.Zero;
        //    if (LinearAlgebraUtil.isEpsilon(velocity))
        //        velocity = Vector3.Zero;
       }

        public void OnCollisionEnter(Collider other)
        {

        }

        public void AddForce(Vector3 force)
        {
            acceleration += force / mass;
        }

        public void AddTorque(Vector3 torque)
        {
            torque += torque / mass;
        }

        public void Reset()
        {
            velocity = Vector3.Zero;
            acceleration = Vector3.Zero;
            torque = Vector3.Zero;
            angularVelocity = Vector3.Zero;
        }

        public override void LoadXml(XmlElement element)
        {
            var velocityNode = element.SelectSingleNode("Velocity") as XmlElement;
            var torqueNode = element.SelectSingleNode("Torque") as XmlElement;
            var angularDragNode = element.SelectSingleNode("AngularDrag");
            var massNode = element.SelectSingleNode("Mass") as XmlElement;
            var dragNode = element.SelectSingleNode("Drag") as XmlElement;
            var kinematicNode = element.SelectSingleNode("IsKinematic") as XmlElement;

            velocity = LinearAlgebraUtil.Vector3FromXml(velocityNode);
            torque = LinearAlgebraUtil.Vector3FromXml(torqueNode);

            if (massNode != null)
                mass = float.Parse(massNode.InnerText);
            if (dragNode != null)
                drag = float.Parse(dragNode.InnerText);
            if (angularDragNode != null)
                angularDrag = float.Parse(angularDragNode.InnerText);
            if (kinematicNode != null)
                isKinematic = bool.Parse(kinematicNode.InnerText);
        }

        public void SaveXml(XmlElement element)
        {
            var velocityNode = LinearAlgebraUtil.Vector3ToXml(element.OwnerDocument, "Velocity", velocity);
            var torqueNode = LinearAlgebraUtil.Vector3ToXml(element.OwnerDocument, "Torque", torque);
            var angularDragNode = element.OwnerDocument.CreateElement("AngularDrag");
            var massNode = element.OwnerDocument.CreateElement("Mass");
            var dragNode = element.OwnerDocument.CreateElement("Drag");
            var kinimaticNode = element.OwnerDocument.CreateElement("IsKinematic");

            angularDragNode.Value = angularDrag.ToString();
            massNode.Value = mass.ToString();
            dragNode.Value = drag.ToString();
            kinimaticNode.Value = isKinematic.ToString();

            element.AppendChild(velocityNode);
            element.AppendChild(torqueNode);
            element.AppendChild(angularDragNode);
            element.AppendChild(massNode);
            element.AppendChild(dragNode);
            element.AppendChild(kinimaticNode);
        }
    }
}
