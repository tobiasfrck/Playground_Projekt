using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Playground_Projekt
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        Vector3 camTarget;
        Vector3 camPosition;
        Matrix projectionMatrix; //3D into 2D
        Matrix viewMatrix; //Location and Orientation of virtual Camera?
        Matrix worldMatrix; //position in Space of objects

        BasicEffect basicEffect;

        //Geometric Info
        VertexPositionColor[] triangleVertices;
        VertexBuffer vertexBuffer;

        //Orbit
        bool orbit;

        //Content:
        private Model model;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            base.Initialize();

            //Setup Camera
            camTarget = new Vector3(0f,0f,0f);
            camPosition = new Vector3(0f,0f,-100f);

            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45),GraphicsDevice.DisplayMode.AspectRatio,1f,1000f);
            
            viewMatrix = Matrix.CreateLookAt(camPosition,camTarget,Vector3.Up);
            
            worldMatrix = Matrix.CreateWorld(new Vector3(0f,0f,0f),Vector3.Forward,Vector3.Up);
            
            //BasicEffect
            basicEffect = new BasicEffect(GraphicsDevice);
            basicEffect.Alpha = 1.0f;

            // Want to see the colors of the vertices, this needs to be on
            basicEffect.VertexColorEnabled = true;

            //Lighting requires normal information which VertexPositionColor does not have
            //If you want to use lighting and VPC you need to create a custom def
            basicEffect.LightingEnabled = false;

            //Geometry  - a simple triangle about the origin
            triangleVertices = new VertexPositionColor[3];
            triangleVertices[0] = new VertexPositionColor(new Vector3(
                                  0, 20, 0), Color.Red);
            triangleVertices[1] = new VertexPositionColor(new Vector3(-
                                  20, -20, 0), Color.Green);
            triangleVertices[2] = new VertexPositionColor(new Vector3(
                                  20, -20, 0), Color.Blue);


        }

        protected override void LoadContent()
        {



            model = Content.Load<Model>("Monkey");
            // TODO: use this.Content to load your game content here
        }



        private void DrawModel(Model model, Matrix world, Matrix view, Matrix projection)
        {
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = world;
                    effect.View = view;
                    effect.Projection = projection;
                }

                mesh.Draw();
            }
        }



        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            if (Keyboard.GetState().IsKeyDown(Keys.Left))
            {
                camPosition.X -= 1f;
                camTarget.X -= 1f;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Right))
            {
                camPosition.X += 1f;
                camTarget.X += 1f;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Up))
            {
                camPosition.Y -= 1f;
                camTarget.Y -= 1f;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Down))
            {
                camPosition.Y += 1f;
                camTarget.Y += 1f;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.D))
            {
                camPosition.Z += 1f;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.C))
            {
                camPosition.Z -= 1f;
            }

            viewMatrix = Matrix.CreateLookAt(camPosition, camTarget,Vector3.Up);
            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            basicEffect.Projection = projectionMatrix;
            basicEffect.View = viewMatrix;
            basicEffect.World = worldMatrix;

            GraphicsDevice.Clear(Color.CornflowerBlue);


            RasterizerState rasterizerState = new RasterizerState();
            rasterizerState.CullMode = CullMode.None;
            GraphicsDevice.RasterizerState = rasterizerState;

            foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                //GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, 3);
            }
            // TODO: Add your drawing code here
            DrawModel(model,worldMatrix,viewMatrix,projectionMatrix);
            base.Draw(gameTime);
        }
    }
}