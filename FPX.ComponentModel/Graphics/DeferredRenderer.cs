using System;
using System.IO;
using System.Xml;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using LodeObj;
using FPX.ComponentModel;

using XnaModel = Microsoft.Xna.Framework.Graphics.Model;

namespace FPX.Visual
{
    public class DeferredRenderer
    {
        public int ScreenWidth
        {
            get { return GameCore.graphicsDevice.Viewport.Width; }
        }

        public int ScreenHeight
        {
            get { return GameCore.graphicsDevice.Viewport.Height; }
        }

        public ContentManager Content;

        public GraphicsDevice Device
        {
            get { return GameCore.graphicsDevice; }
        }

        private PrimitiveType PrimitiveType { get { return Settings.GetSetting<PrimitiveType>("FillMode"); } }

        public SamplerState anisoSampler;
        private BlendState lightBlend;

        RenderTarget2D diffuseMap;
        RenderTarget2D normalMap;
        RenderTarget2D specularMap;
        RenderTarget2D depthMap;
        RenderTarget2D occlusionMap;

        RenderTargetBinding[] rt_bindings;
        RenderTargetBinding[] null_bindings;

        Effect GBufferShader;
        Effect DirectionalLightShader;
        Effect AmbientLightShader;
        Effect PointLightShader;
        BasicEffect basicEffect;

        VertexBuffer TestQuad;
        VertexBuffer ScreenQuad;
        VertexBuffer vBuffer;

        VertexPositionTexture[] testQuadVertecies;

        public DeferredRenderer()
        {
            Content = GameCore.content;
            DeviceUpdate(GameCore.graphicsDevice.Viewport.Width, GameCore.graphicsDevice.Viewport.Height);
            GameCore.graphicsDevice.DeviceReset += GraphicsDevice_DeviceReset;

            GBufferShader = Content.Load<Effect>("Shaders\\DeferredGBuffers");
            AmbientLightShader = Content.Load<Effect>("Shaders\\AmbientLight");
            DirectionalLightShader = Content.Load<Effect>("Shaders\\DirectionalLight");
            PointLightShader = Content.Load<Effect>("Shaders\\PointLight");

            // GBufferShader = Content.Load<Effect>("Shaders\\DeferredGBuffers");
            // DirectionalLightShader = Content.Load<Effect>("Shaders\\Lights\\DirectionalLight");
            // AmbientLightShader = Content.Load<Effect>("Shaders\\Lights\\AmbientLight");
            basicEffect = new BasicEffect(Device);
            basicEffect.TextureEnabled = true;

            testQuadVertecies = new VertexPositionTexture[]
            {
                new VertexPositionTexture(new Vector3(-0.5f, -0.5f, 0.0f), new Vector2(0.0f, 1.0f)),
                new VertexPositionTexture(new Vector3(-0.5f, 0.5f, 0.0f),new Vector2(0.0f, 0.0f)),
                new VertexPositionTexture(new Vector3(0.5f, -0.5f, 0.0f),new Vector2(1.0f, 1.0f)),
                new VertexPositionTexture(new Vector3(0.5f, 0.5f, 0.0f),new Vector2(1.0f,0.0f)),
            };

            VertexPositionTexture[] screenQuadVertecies = new VertexPositionTexture[]
            {
                new VertexPositionTexture(new Vector3(-1, -1, 0.0f), new Vector2(0.0f, 1.0f)),
                new VertexPositionTexture(new Vector3(-1, 1, 0.0f),new Vector2(0.0f, 0.0f)),
                new VertexPositionTexture(new Vector3(1, -1, 0.0f),new Vector2(1.0f, 1.0f)),
                new VertexPositionTexture(new Vector3(1, 1, 0.0f),new Vector2(1.0f,0.0f)),
            };

            TestQuad = new VertexBuffer(Device, VertexPositionTexture.VertexDeclaration, 4, BufferUsage.None);
            TestQuad.SetData<VertexPositionTexture>(testQuadVertecies);

            ScreenQuad = new VertexBuffer(Device, VertexPositionTexture.VertexDeclaration, 4, BufferUsage.None);
            ScreenQuad.SetData<VertexPositionTexture>(screenQuadVertecies);

            vBuffer = new VertexBuffer(Device, typeof(VertexPositionNormalTextureBinormal), 1, BufferUsage.None);

            anisoSampler = new SamplerState();
            anisoSampler.AddressU = TextureAddressMode.Wrap;
            anisoSampler.AddressV = TextureAddressMode.Wrap;
            anisoSampler.AddressW = TextureAddressMode.Wrap;

            anisoSampler.Filter = TextureFilter.PointMipLinear;
            anisoSampler.MaxAnisotropy = 16;
            anisoSampler.MaxMipLevel = 4;
            anisoSampler.FilterMode = TextureFilterMode.Comparison;

            lightBlend = new BlendState();

            Device.SamplerStates[0] = Device.SamplerStates[1] = Device.SamplerStates[2] = Device.SamplerStates[3] = anisoSampler;

            Debug.Log("Sampler State: " + Device.SamplerStates[3].Filter.ToString());
        }

        private void GraphicsDevice_DeviceReset(object sender, EventArgs e)
        {
            DeviceUpdate(GameCore.viewport.Width, GameCore.viewport.Height);
        }

        private void DeviceUpdate(int ScreenWidth, int ScreenHeight)
        {
            if (ScreenHeight == 0 || ScreenWidth == 0)
                return;

            if (diffuseMap != null) diffuseMap.Dispose();
            if (normalMap != null) normalMap.Dispose();
            if (specularMap != null) specularMap.Dispose();
            if (depthMap != null) depthMap.Dispose();

            diffuseMap = new RenderTarget2D(Device, ScreenWidth, ScreenHeight, false, SurfaceFormat.Rgba64, DepthFormat.Depth24Stencil8, 0, RenderTargetUsage.PreserveContents);
            diffuseMap.Name = "Diffuse Map";
            normalMap = new RenderTarget2D(Device, ScreenWidth, ScreenHeight, false, SurfaceFormat.Rgba64, DepthFormat.Depth24Stencil8, 0, RenderTargetUsage.PreserveContents);
            normalMap.Name = "Normal Map";
            specularMap = new RenderTarget2D(Device, ScreenWidth, ScreenHeight, false, SurfaceFormat.Rgba64, DepthFormat.Depth24Stencil8, 0, RenderTargetUsage.PreserveContents);
            specularMap.Name = "Specular Map";
            depthMap = new RenderTarget2D(Device, ScreenWidth, ScreenHeight, false, SurfaceFormat.Vector4, DepthFormat.Depth24Stencil8, 0, RenderTargetUsage.PreserveContents);
            depthMap.Name = "Depth Map";
            occlusionMap = new RenderTarget2D(Device, ScreenWidth, ScreenHeight, false, SurfaceFormat.Vector4, DepthFormat.Depth24Stencil8, 0, RenderTargetUsage.PreserveContents);
            occlusionMap.Name = "Occlusion Map";

            rt_bindings = new RenderTargetBinding[4]
            {
                diffuseMap,
                normalMap,
                specularMap,
                depthMap,
            };
            Device.SetRenderTargets(rt_bindings);
        }

        public void RenderGeometry(Camera camera = null)
        {
            if (camera != null)
            {
                GBufferShader.Parameters["ViewProjection"].SetValue(camera.ViewMatrix * camera.ProjectionMatrix);
                Vector3 cameraForward = camera.transform.forward;
                cameraForward.Normalize();
                GBufferShader.Parameters["CameraForward"].SetValue(cameraForward);
            }

            foreach (MeshRenderer meshRender in Component.g_collection.FindAll(c => c is MeshRenderer))
                RenderObject(meshRender);

            var renderables = Component.g_collection.FindAll(c => c is IDrawable && c.GetType() != typeof(MeshRenderer) && c.GetType() != typeof(SkyboxRenderer)).Cast<IDrawable>().ToList();
            renderables.ToList().Sort(Graphics.SortRenderables);
            foreach (IDrawable drawable in renderables)
            {
                if (!drawable.Visible)
                    continue;

                drawable.Draw(Time.GameTime);
            }
        }

        public void Draw(GameTime gameTime)
        {
            if (GameCore.currentLevel == null)
            {
                for (int i = 0; i < 8; i++)
                    Device.SetRenderTargets(null);
                return;
            }

            BeginRenderGBuffers(Camera.Active);

            RenderGeometry();

            EndRenderGBuffers();

            var postProcessor = Camera.Active.GetComponent<PostProcessor>();
            if (postProcessor != null) postProcessor.Begin();
            {
                var lights = Component.g_collection.FindAll(c => c is Light).Cast<Light>().ToList();
                lights.ForEach(l => l.RenderShadows());
                RenderLights(lights);

                var skyboxes = Component.g_collection.FindAll(c => c is SkyboxRenderer);
                foreach (SkyboxRenderer skybox in skyboxes)
                    skybox.Draw(gameTime);
            }
            if (postProcessor != null) postProcessor.End();
        }

        public void BeginRenderGBuffers(Camera camera = null)
        {
            if (camera == null)
                camera = Camera.Active;

            Device.SetRenderTargets(rt_bindings);

            GBufferShader.Parameters["ViewProjection"].SetValue(camera.ViewMatrix * camera.ProjectionMatrix);
            Vector3 cameraForward = camera.transform.forward;
            cameraForward.Normalize();
            GBufferShader.Parameters["CameraForward"].SetValue(cameraForward);

            GBufferShader.CurrentTechnique.Passes[0].Apply();

            Device.Clear(Color.Transparent);
            // Device.SetVertexBuffer(vBuffer);
            Device.BlendState = BlendState.Opaque;
        }

        public void RenderObject(MeshRenderer renderer)
        {
            if (!renderer.Visible || !renderer.gameObject.Visible)
                return;

            Material material = renderer.GetComponent<Material>();
            GBufferShader.Parameters["DiffuseMap"].SetValue(material.DiffuseMap);
            GBufferShader.Parameters["NormalMap"].SetValue(material.NormalMap);
            GBufferShader.Parameters["SpecularMap"].SetValue(material.SpecularMap);

            GBufferShader.Parameters["World"].SetValue(renderer.transform.worldPose);
            GBufferShader.Parameters["DiffuseColor"].SetValue(renderer.GetComponent<Material>().DiffuseColor.ToVector4());
            GBufferShader.Parameters["SpecularColor"].SetValue(renderer.GetComponent<Material>().SpecularColor.ToVector4());
            GBufferShader.Parameters["Roughness"].SetValue(material.Roughness);
            GBufferShader.Parameters["SpecularPower"].SetValue(material.SpecularPower / 10.0f);
            GBufferShader.Parameters["SpecularIntensity"].SetValue(material.SpecularIntensity / 10.0f);
            //Device.Textures[0] = renderer.GetComponent<Material>().DiffuseMap;
            //Device.Textures[1] = renderer.GetComponent<Material>().NormalMap;
            //Device.Textures[2] = renderer.GetComponent<Material>().SpecularMap;
            GBufferShader.CurrentTechnique.Passes[0].Apply();

            // Device.DrawPrimitives(PrimitiveType.TriangleList, renderer.startIndex, renderer.primitiveCount);
            Device.DrawUserIndexedPrimitives(PrimitiveType, renderer.Vertecies, 0, renderer.Vertecies.Length, renderer.Indicies, 0, renderer.primitiveCount);
        }

        public void EndRenderGBuffers()
        {
            for (int i = 0; i < 8; i++)
                Device.Textures[i] = null;

            Device.SetRenderTargets(null);
        }

        public void RenderShadows(List<Light> lights)
        {
            lights.ForEach(l => l.RenderShadows());
        }

        public void RenderLights(List<Light> Lights)
        {
            var camera = Camera.Active;

            Device.Clear(Color.Black);
            Device.SetVertexBuffer(ScreenQuad);
            Device.BlendState = BlendState.Additive;
            Device.SamplerStates[0] = SamplerState.AnisotropicWrap;
            Device.SamplerStates[1] = SamplerState.AnisotropicWrap;
            Device.SamplerStates[2] = SamplerState.AnisotropicWrap;
            Device.SamplerStates[3] = SamplerState.AnisotropicWrap;

            foreach (Light light in Lights.FindAll(l => l.LightType == LightType.Directional))
            {
                DirectionalLightShader.Parameters["DiffuseColor"].SetValue(light.DiffuseColor.ToVector4());
                DirectionalLightShader.Parameters["SpecularColor"].SetValue(light.SpecularColor.ToVector4());
                DirectionalLightShader.Parameters["gCameraPos"].SetValue(camera.position);
                DirectionalLightShader.Parameters["Intensity"].SetValue(light.Intensity);
                DirectionalLightShader.Parameters["gInvViewProj"].SetValue(Matrix.Invert(camera.ViewMatrix * camera.ProjectionMatrix));
                DirectionalLightShader.Parameters["LightDirection"].SetValue(light.transform.forward);

                DirectionalLightShader.CurrentTechnique.Passes[0].Apply();

                Device.Textures[0] = diffuseMap;
                Device.Textures[1] = normalMap;
                Device.Textures[2] = specularMap;
                Device.Textures[3] = depthMap;

                Device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
            }

            foreach (Light pointLight in Lights.FindAll(l => l.LightType == LightType.Point))
            {
                PointLightShader.Parameters["DiffuseColor"].SetValue(pointLight.DiffuseColor.ToVector4());
                PointLightShader.Parameters["SpecularColor"].SetValue(pointLight.SpecularColor.ToVector4());
                PointLightShader.Parameters["gCameraPos"].SetValue(camera.position);
                PointLightShader.Parameters["Intensity"].SetValue(pointLight.Intensity);
                PointLightShader.Parameters["gInvViewProj"].SetValue(Matrix.Invert(camera.ViewMatrix * camera.ProjectionMatrix));
                PointLightShader.Parameters["LightPosition"].SetValue(pointLight.position);
                PointLightShader.Parameters["Range"].SetValue(pointLight.Range);

                PointLightShader.CurrentTechnique.Passes[0].Apply();

                Device.Textures[0] = diffuseMap;
                Device.Textures[1] = normalMap;
                Device.Textures[2] = specularMap;
                Device.Textures[3] = depthMap;

                Device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
            }

            foreach (Light light in Lights.FindAll(l => l.LightType == LightType.Ambient))
            {
                AmbientLightShader.Parameters["DiffuseColor"].SetValue(light.DiffuseColor.ToVector4());
                AmbientLightShader.Parameters["SpecularColor"].SetValue(light.SpecularColor.ToVector4());
                AmbientLightShader.Parameters["gCameraPos"].SetValue(Camera.Active.transform.position);
                AmbientLightShader.Parameters["Intensity"].SetValue(light.Intensity);

                AmbientLightShader.CurrentTechnique.Passes[0].Apply();

                Device.Textures[0] = diffuseMap;
                Device.Textures[1] = normalMap;
                Device.Textures[2] = specularMap;
                Device.Textures[3] = depthMap;

                Device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
            }

            for (int i = 0; i < 4; i++)
                Device.Textures[i] = null;

        }

        public void AppendVertecies(VertexPositionNormalTextureBinormal[] vertecies, out int startIndex)
        {
            startIndex = vBuffer.VertexCount;
            VertexPositionNormalTextureBinormal[] gpu_vertecies = new VertexPositionNormalTextureBinormal[vBuffer.VertexCount];
            vBuffer.GetData(gpu_vertecies);
            VertexPositionNormalTextureBinormal[] newVBuffer = new VertexPositionNormalTextureBinormal[gpu_vertecies.Length + vertecies.Length];

            for (int i = 0; i < newVBuffer.Length; i++)
            {
                if (i < gpu_vertecies.Length)
                    newVBuffer[i] = gpu_vertecies[i];
                else
                    newVBuffer[i] = vertecies[i - gpu_vertecies.Length];
            }

            vBuffer = new VertexBuffer(Device, typeof(VertexPositionNormalTextureBinormal), newVBuffer.Length, BufferUsage.None);
            vBuffer.SetData(newVBuffer);
            startIndex = 0;
        }

        public void SetVBufferXml(XmlElement geometryNode)
        {
            var positionNode = geometryNode.ChildNodes.Cast<XmlNode>().ToList().Find(n => n.Name == "Positions");
            var normalNode = geometryNode.ChildNodes.Cast<XmlNode>().ToList().Find(n => n.Name == "Normals");
            var uvsNode = geometryNode.ChildNodes.Cast<XmlNode>().ToList().Find(n => n.Name == "Uvs");
            var binormalsNode = geometryNode.ChildNodes.Cast<XmlNode>().ToList().Find(n => n.Name == "Binormals");

            string positionString = positionNode.InnerText;
            string normalString = normalNode.InnerText;
            string uvsString = uvsNode.InnerText;
            string binormalsString = binormalsNode.InnerText;

            string[] positionStringData = positionString.Split(new char[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
            string[] normalStringData = normalString.Split(new char[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
            string[] uvsStringData = uvsString.Split(new char[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
            string[] binormalsStringData = binormalsString.Split(new char[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);

            VertexPositionNormalTextureBinormal[] gpu_vertecies = new VertexPositionNormalTextureBinormal[positionStringData.Length / 3];
            for (int i = 0; i < gpu_vertecies.Length; i++)
            {
                int float3Index = i * 3;
                int float2Index = i * 2;

                gpu_vertecies[i].Position = new Vector4(float.Parse(positionStringData[float3Index]), float.Parse(positionStringData[float3Index + 1]), float.Parse(positionStringData[float3Index + 2]), 0.0f);
                gpu_vertecies[i].Normal = new Vector3(float.Parse(normalStringData[float3Index]), float.Parse(normalStringData[float3Index + 1]), float.Parse(normalStringData[float3Index + 2]));
                gpu_vertecies[i].Binormal = new Vector3(float.Parse(binormalsStringData[float3Index]), float.Parse(binormalsStringData[float3Index + 1]), float.Parse(binormalsStringData[float3Index + 2]));
                gpu_vertecies[i].TextureCoordinate = new Vector2(float.Parse(binormalsStringData[float2Index]), float.Parse(binormalsStringData[float2Index + 1]));

            }

            vBuffer = new VertexBuffer(Device, typeof(VertexPositionNormalTextureBinormal), gpu_vertecies.Length, BufferUsage.None);
            vBuffer.SetData(gpu_vertecies);
        }

        public void GetVBufferXml(XmlNode parent)
        {
            VertexPositionNormalTextureBinormal[] gpu_vertecies = new VertexPositionNormalTextureBinormal[vBuffer.VertexCount];
            vBuffer.GetData(gpu_vertecies);

            string positionString = "",
                uvString = "",
                normalString = "",
                binormalString = "";

            for (int i = 0; i < gpu_vertecies.Length; i++)
            {
                positionString += string.Format("{0}, {1}, {2},", gpu_vertecies[i].Position.X, gpu_vertecies[i].Position.Y, gpu_vertecies[i].Position.Z);
                uvString += string.Format("{0}, {1},", gpu_vertecies[i].TextureCoordinate.X, gpu_vertecies[i].TextureCoordinate.Y);
                normalString += string.Format("{0}, {1}, {2},", gpu_vertecies[i].Normal.X, gpu_vertecies[i].Normal.Y, gpu_vertecies[i].Normal.Z);
                binormalString += string.Format("{0}, {1}, {2},", gpu_vertecies[i].Binormal.X, gpu_vertecies[i].Binormal.Y, gpu_vertecies[i].Binormal.Z);
            }

            var geometryNode = parent.AppendChild(parent.OwnerDocument.CreateElement("Geometry"));
            var positionsNode = geometryNode.AppendChild(parent.OwnerDocument.CreateElement("Positions"));
            var normalsNode = geometryNode.AppendChild(parent.OwnerDocument.CreateElement("Normals"));
            var uvsNode = geometryNode.AppendChild(parent.OwnerDocument.CreateElement("Uvs"));
            var binormalsNode = geometryNode.AppendChild(parent.OwnerDocument.CreateElement("Binormals"));

            positionsNode.InnerText = positionString;
            normalsNode.InnerText = normalString;
            uvsNode.InnerText = uvString;
            binormalsNode.InnerText = binormalString;
        }

        public void _debug_renderGBufferResults()
        {
            Device.Clear(Camera.Active.ClearColor);

            Device.BlendState = BlendState.Opaque;
            Device.SetVertexBuffer(TestQuad);
            basicEffect.DiffuseColor = Color.White.ToVector3();
            basicEffect.TextureEnabled = true;

            basicEffect.Projection = Matrix.Identity;
            basicEffect.View = Matrix.Identity;

            basicEffect.World = Matrix.CreateTranslation((Vector3.Up + Vector3.Left) * 0.5f);
            basicEffect.Texture = diffuseMap;
            basicEffect.CurrentTechnique.Passes[0].Apply();

            Device.DrawUserPrimitives(PrimitiveType.TriangleStrip, testQuadVertecies, 0, 2);

            basicEffect.World = Matrix.CreateTranslation((Vector3.Up + Vector3.Right) * 0.5f);
            basicEffect.Texture = normalMap;
            basicEffect.CurrentTechnique.Passes[0].Apply();

            Device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);

            basicEffect.World = Matrix.CreateTranslation((Vector3.Down + Vector3.Left) * 0.5f);
            basicEffect.Texture = specularMap;
            basicEffect.CurrentTechnique.Passes[0].Apply();

            Device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);

            basicEffect.World = Matrix.CreateTranslation((Vector3.Down + Vector3.Right) * 0.5f);
            basicEffect.Texture = depthMap;
            basicEffect.CurrentTechnique.Passes[0].Apply();

            Device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);

            Device.SetVertexBuffer(null);
        }

        public void _debug_OutuptGBuffers()
        {
            GameCore.content.Load<Texture2D>("Textures/TestTexture").SaveAsPng(new FileStream(Environment.CurrentDirectory + "\\testOutput.png", FileMode.Create), ScreenWidth, ScreenHeight);
            Debug.Log("Output test");
            diffuseMap.SaveAsPng(new FileStream(Environment.CurrentDirectory + "\\diffuseMapOutput.png", FileMode.Create), ScreenWidth, ScreenHeight);
            Debug.Log("Output diffuse");
            normalMap.SaveAsPng(new FileStream(Environment.CurrentDirectory + "\\normalMapOutput.png", FileMode.Create), ScreenWidth, ScreenHeight);
            Debug.Log("Output Normals");
            specularMap.SaveAsPng(new FileStream(Environment.CurrentDirectory + "\\specularMapOutput.png", FileMode.Create), ScreenWidth, ScreenHeight);
            Debug.Log("Output Specular");
            depthMap.SaveAsPng(new FileStream(Environment.CurrentDirectory + "\\depthMapOutput.png", FileMode.Create), ScreenWidth, ScreenHeight);
            Debug.Log("Output Depth");
        }
    }



    public static class GraphcisDeviceExtentions
    {
        public static void OutputActiveTextures(this GraphicsDevice Device, string fileNamePreface = "DeviceTexture")
        {
            for (int i = 0; i < 16; i++)
            {
                try
                {
                    Texture2D tex = Device.Textures[i] as Texture2D;
                    string filename = string.Format(Environment.CurrentDirectory + "\\{0}{1}.jpg", fileNamePreface, i.ToString());
                    tex.SaveAsJpeg(new FileStream(filename, FileMode.Create), GameCore.graphicsDevice.Viewport.Width, GameCore.graphicsDevice.Viewport.Height);
                    Console.WriteLine("Wrote device texture{0} to {1}", i, filename);
                }
                catch (NullReferenceException)
                {
                    Console.WriteLine("Device does not have texuture{0} set", i);
                    continue;
                }
            }
        }
    }
}
