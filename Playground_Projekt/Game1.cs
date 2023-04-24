using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Diagnostics;
using System.Transactions;

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


        //Content:
        public Model ringModel;
        private Model plane;
        private Ring[] rings;

        //Gameplay-Variables
        private int score = 0;

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

            //Generate Rings
            Random rnd = new Random();
            rings = new Ring[rnd.Next(5,10)];
            int distance = 30;
            int maxDistance = distance+50;
            for (int i = 0; i < rings.Length; i++)
            {
                Vector3 ringPosition = Vector3.Zero;
                if (i == 0)
                {
                    ringPosition = new Vector3(rnd.Next(-45, 45), 0, distance);
                    rings[0] = new Ring(ringPosition, rnd.Next(2, 50), false);
                } else {
                    ringPosition = new Vector3(rnd.Next(-45, 45), 0, rings[i - 1].getZ() + (rnd.Next(distance, maxDistance)));
                    rings[i] = new Ring(ringPosition,rnd.Next(2,50),false);
                }
                rings[i].setBoundingBox(calculateBoundingBox(ringModel, Matrix.CreateTranslation(ringPosition)));
            }

        }

        protected override void LoadContent()
        {
            // TODO: use this.Content to load your game content here
            ringModel = Content.Load<Model>("Ring");
            plane = Content.Load<Model>("plane");
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
            if (Keyboard.GetState().IsKeyDown(Keys.Right))
            {
                camPosition.X -= 1f;
                camTarget.X -= 1f;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Left))
            {
                camPosition.X += 1f;
                camTarget.X += 1f;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Up))
            {
                camPosition.Y += 1f;
                camTarget.Y += 1f;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Down))
            {
                camPosition.Y -= 1f;
                camTarget.Y -= 1f;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.D))
            {
            }
                camPosition.Z += 1f;
                camTarget.Z += 1f;
            if (Keyboard.GetState().IsKeyDown(Keys.C))
            {
                camPosition.Z -= 1f;
                camTarget.Z -= 1f;
            }

            if (camPosition.X<-50)
            {
                camTarget.X = -50;
                camPosition.X = -50;
            }
            else if (camPosition.X>50)
            {
                camTarget.X = 50;
                camPosition.X=50;
            }
            viewMatrix = Matrix.CreateLookAt(camPosition, camTarget,Vector3.Up);
            // TODO: Add your update logic here
            Debug.WriteLine(score);

            //PointsManager
            for(int i = 0; i < rings.Length; i++)
            {
                if (rings[i].IsColliding(camPosition))
                {
                    score += rings[i].points;
                    rings[i].dead = true;
                }
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            RasterizerState rasterizerState = new RasterizerState();
            rasterizerState.CullMode = CullMode.None;
            GraphicsDevice.RasterizerState = rasterizerState;


            // TODO: Add your drawing code here
            for(int i = 0; i < rings.Length; i++)
            {
                DrawModel(ringModel, Matrix.CreateTranslation(rings[i].position), viewMatrix,projectionMatrix);
            }

            //DrawModel(plane, worldMatrix, viewMatrix, projectionMatrix);

            base.Draw(gameTime);
        }
        public BoundingBox calculateBoundingBox(Model model, Matrix worldTransform)
        {
            Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
            foreach(ModelMesh mesh in model.Meshes)
            {
                
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    int vertexStride = part.VertexBuffer.VertexDeclaration.VertexStride;
                    int vertexBufferSize = part.NumVertices * vertexStride;

                    float[] vertexData = new float[vertexBufferSize/sizeof(float)];
                    part.VertexBuffer.GetData<float>(vertexData);

                    for(int i = 0;i < vertexBufferSize / sizeof(float); i+=vertexStride/sizeof(float))
                    {
                        Vector3 vertexPosition = new Vector3(vertexData[i], vertexData[i+1], vertexData[i+2]);
                        min = Vector3.Min(min, vertexPosition);
                        max = Vector3.Max(max, vertexPosition);
                    }
                }
            }
            min = Vector3.Transform(min, worldTransform);
            max = Vector3.Transform(max, worldTransform);
            return new BoundingBox(min, max);
        }
    }
    public class Ring
    {
        public Vector3 position;
        public int points;
        public bool win;
        public bool dead = false;
        BoundingBox box;
        public void setBoundingBox(BoundingBox bbox)
        {
            box = bbox;
        }
        public Ring(Vector3 pos, int points, bool win)
        {
            position = pos;
            this.points = points;
            this.win = win;
        }
        public float getZ()
        {
            return position.Z;
        }
        public void Update(GameTime gameTime) {
            if (dead) return;


        }
        public void Draw(GameTime gameTime) { 
            if (dead) return;

        }
        public bool IsColliding(Vector3 pos)
        {
            if (!dead)
            {
                if(box.Contains(pos).Equals(ContainmentType.Contains))
                {
                    return true;
                }
            }
            return false;
        }
        
    }
}