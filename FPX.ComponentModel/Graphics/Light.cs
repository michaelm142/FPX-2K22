using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FPX.ComponentModel;
using FPX.Editor;

namespace FPX.Visual
{
    [Editor(typeof(LightEditor))]
    public class Light : Component, ILightSource
    {
        private static GameObject shadowCam;

        public Color DiffuseColor { get; set; }

        public Color SpecularColor { get; set; }

        public float SpecularIntensity { get; set; } = 0.0f;
        public float SpecularPower { get; set; } = 0.2f;
        public float Intensity { get; set; } = 1.0f;
        public float Range { get; set; } = 5.0f;

        public int ShadowMapSize { get; set; } = 1024;

        public LightType LightType { get; set; }

        public bool UseShadows;

        public RenderTarget2D shadowMap { get; private set; }
        public RenderTargetCube shadowCube { get; private set; }

        public static Color DefaultColor { get { return Color.Gray; } }

        public override void LoadXml(XmlElement element)
        {
            if (shadowCam == null)
            {
                shadowCam = new GameObject();
                shadowCam.Name = "Shadow Camera";
                var cam = shadowCam.AddComponent<Camera>();
                cam.nearPlaneDistance = 0.01f;
            }

            XmlElement diffuseColorElement = element.SelectSingleNode("DiffuseColor") as XmlElement;
            XmlElement specularColorElement = element.SelectSingleNode("SpecularColor") as XmlElement;
            XmlElement specularIntensityelement = element.SelectSingleNode("SpecularIntensity") as XmlElement;
            XmlElement specularPowerelement = element.SelectSingleNode("SpecularPower") as XmlElement;
            XmlElement typeElement = element.SelectSingleNode("LightType") as XmlElement;
            XmlElement intensityElement = element.SelectSingleNode("Intensity") as XmlElement;
            XmlElement rangeElement = element.SelectSingleNode("Range") as XmlElement;
            XmlElement useShadowsElement = element.SelectSingleNode("UseShadows") as XmlElement;

            if (diffuseColorElement != null)
                DiffuseColor = LinearAlgebraUtil.ColorFromXml(diffuseColorElement);
            if (specularColorElement != null)
                SpecularColor = LinearAlgebraUtil.ColorFromXml(specularColorElement);
            if (specularIntensityelement != null)
                SpecularIntensity = float.Parse(specularIntensityelement.InnerText);
            if (specularPowerelement != null)
                SpecularPower = float.Parse(specularPowerelement.InnerText);
            if (typeElement != null)
                LightType = (LightType)Enum.Parse(typeof(LightType), typeElement.InnerText);
            if (intensityElement != null)
                Intensity = float.Parse(intensityElement.InnerText);
            if (rangeElement != null)
                Range = float.Parse(rangeElement.InnerText);
            if (useShadowsElement != null)
                UseShadows = bool.Parse(useShadowsElement.InnerText);

            if (UseShadows)
                CreateShadowMaps();
        }

        private void CreateShadowMaps()
        {
            switch (LightType)
            {
                case LightType.Directional:
                    shadowMap = new RenderTarget2D(GameCore.graphicsDevice, ShadowMapSize, ShadowMapSize);
                    break;
                case LightType.Point:
                    shadowCube = new RenderTargetCube(GameCore.graphicsDevice, ShadowMapSize, true, SurfaceFormat.Single, DepthFormat.Depth16);
                    break;

            }
        }

        public void RenderShadows()
        {
            var device = GameCore.graphicsDevice;

            shadowCam.position = position;

            foreach (CubeMapFace face in Enum.GetValues(typeof(CubeMapFace)))
            {
                switch (face)
                {
                    case CubeMapFace.NegativeX:
                        shadowCam.rotation = Quaternion.CreateFromRotationMatrix(Matrix.CreateLookAt(Vector3.Zero, -transform.right, transform.up));
                        break;
                    case CubeMapFace.PositiveX:
                        shadowCam.rotation = Quaternion.CreateFromRotationMatrix(Matrix.CreateLookAt(Vector3.Zero, transform.right, transform.up));
                        break;
                    case CubeMapFace.NegativeY:
                        shadowCam.rotation = Quaternion.CreateFromRotationMatrix(Matrix.CreateLookAt(Vector3.Zero, -transform.up, transform.forward));
                        break;
                    case CubeMapFace.PositiveY:
                        shadowCam.rotation = Quaternion.CreateFromRotationMatrix(Matrix.CreateLookAt(Vector3.Zero, transform.up, transform.forward));
                        break;
                    case CubeMapFace.NegativeZ:
                        shadowCam.rotation = Quaternion.CreateFromRotationMatrix(Matrix.CreateLookAt(Vector3.Zero, -transform.forward, transform.up));
                        break;
                    case CubeMapFace.PositiveZ:
                        shadowCam.rotation = Quaternion.CreateFromRotationMatrix(Matrix.CreateLookAt(Vector3.Zero, transform.forward, transform.up));
                        break;
                }

                device.SetRenderTarget(shadowCube, face);
                device.Clear(ClearOptions.DepthBuffer, Color.Transparent, 1.0f, 1);
                Graphics.instance.RenderScene(shadowCam.GetComponent<Camera>());
                device.SetRenderTarget(null);
            }
        }
    }

    public enum LightType
    {
        Ambient,
        Point,
        Directional,
    }
}
