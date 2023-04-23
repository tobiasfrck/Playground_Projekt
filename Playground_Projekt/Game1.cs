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

        }

        protected override void LoadContent()
        {
            

            // TODO: use this.Content to load your game content here
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }
    }
}