using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Timers;
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
        public Model spaceShip;
        Vector3 shipPosition;
        public BoundingBox spaceShipBox;
        private Ring[] rings2;
        private List<Ring> rings;
        private SoundEffect collectSFX;
        private SoundEffect missedSFX;
        private SoundEffect winSFX;
        Texture2D buttonSprite;
        private SpriteBatch _spriteBatch;
        private SpriteFont font;

        //Gameplay-Variables
        private int score = 0;
        public int startRingCount = 10;
        public int distance = 30;
        public int maxDistance;
        public Random rnd = new Random();
        public System.Timers.Timer timer1;
        public bool allowMovement = true;
        public ButtonState prevLeftMouseState;
        public String scoreText = "";

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

            
            prevLeftMouseState = ButtonState.Released;
            maxDistance = distance + 50;
            timer1 = new System.Timers.Timer(30000);
            timer1.Elapsed += OnTimedEvent;
            timer1.Start();

            //Setup Camera
            camTarget = new Vector3(0f,0f,0f);
            camPosition = new Vector3(0f,2f,-100f);

            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45),GraphicsDevice.DisplayMode.AspectRatio,1f,1000f);
            
            viewMatrix = Matrix.CreateLookAt(camPosition,camTarget,Vector3.Up);
            
            //worldMatrix = Matrix.CreateWorld(new Vector3(0f,0f,0f),Vector3.Forward,Vector3.Up);

            //Generate Rings
            rings = new List<Ring>();
            for (int i = 0; i < startRingCount; i++)
            {
                Vector3 ringPosition = Vector3.Zero;
                if (i == 0)
                {
                    ringPosition = new Vector3(rnd.Next(-45, 45), 0, distance);
                    rings.Add(new Ring(ringPosition, rnd.Next(25, 50), false));
                } else {
                    ringPosition = new Vector3(rnd.Next(-45, 45), 0, rings[i-1].getZ() + (rnd.Next(distance, maxDistance)));
                    rings.Add(new Ring(ringPosition, rnd.Next(25, 50), false));
                }
                rings[i].setBoundingBox(calculateBoundingBox(ringModel, Matrix.CreateTranslation(ringPosition)));
            }
            
        }


        private void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            timer1.Stop();
            winSFX.Play(0.25f, 0f, 0f);
            if (score>600)
            {
                Debug.WriteLine("You won! Points: " + score);
                scoreText = "You won! Points: " + score;
            }
            else
            {
                Debug.WriteLine("You did not get enough points :( Points: " + score);
                scoreText = "You did not get enough points :( Points: " + score;
            }
            timer1.AutoReset=false;
            allowMovement = false;
        }
        protected override void LoadContent()
        {
            // TODO: use this.Content to load your game content here
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            buttonSprite = Content.Load<Texture2D>("restartButtonSprite");
            font = Content.Load<SpriteFont>("Score");
            ringModel = Content.Load<Model>("Ring");
            spaceShip = Content.Load<Model>("spaceShip");
            collectSFX = Content.Load<SoundEffect>("beep1");
            missedSFX = Content.Load<SoundEffect>("woosh");
            winSFX = Content.Load<SoundEffect>("intro");
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
                    effect.EnableDefaultLighting();
                }
                mesh.Draw();
            }
        }



        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            if (Keyboard.GetState().IsKeyDown(Keys.Right) && allowMovement)
            {
                camPosition.X -= 1f;
                camTarget.X -= 1f;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Left) && allowMovement)
            {
                camPosition.X += 1f;
                camTarget.X += 1f;
            }

            if (!allowMovement && Mouse.GetState().LeftButton == ButtonState.Pressed && prevLeftMouseState==ButtonState.Released)
            {
                if (Mouse.GetState().X <= 120 && Mouse.GetState().X >= 0 && Mouse.GetState().Y <= 80 && Mouse.GetState().Y >= 0)
                {
                    Debug.WriteLine("Restarted game!");
                    allowMovement = true;
                    timer1.Start();
                    score = 0;
                }
            }
            prevLeftMouseState = Mouse.GetState().LeftButton;

            if (allowMovement)
            {
                camPosition.Z += 1f;
                camTarget.Z += 1f;
                scoreText = "Score: " + score;
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
            shipPosition = Vector3.Add(camPosition, new Vector3(0f, -2f, 10f));
            spaceShipBox = calculateBoundingBox(spaceShip, Matrix.CreateTranslation(shipPosition));

            // TODO: Add your update logic here
            //Debug.WriteLine(score);

            //PointsManager
            for(int i = 0; i < rings.Count; i++)
            {
                Vector3 ringPosition = Vector3.Zero;
                if (rings.ElementAt(i).IsColliding(shipPosition))
                {
                    score += rings.ElementAt(i).points;
                    rings.ElementAt(i).dead = true;
                    rings.RemoveAt(i);
                    collectSFX.Play(0.25f,0f,0f);
                    ringPosition = new Vector3(rnd.Next(-45, 45), 0, rings[rings.Count - 1].getZ() + (rnd.Next(distance, maxDistance)));
                    rings.Add(new Ring(ringPosition, rnd.Next(2, 50), false));
                    rings[rings.Count - 1].setBoundingBox(calculateBoundingBox(ringModel, Matrix.CreateTranslation(ringPosition)));
                }
                else if (camPosition.Z-10f>= rings.ElementAt(i).position.Z) {
                    rings.ElementAt(i).dead=true;
                    rings.RemoveAt(i);
                    missedSFX.Play(0.25f, 0f, 0f);
                    ringPosition = new Vector3(rnd.Next(-45, 45), 0, rings[rings.Count - 1].getZ() + (rnd.Next(distance, maxDistance)));
                    rings.Add(new Ring(ringPosition, rnd.Next(2, 50), false));
                    rings[rings.Count - 1].setBoundingBox(calculateBoundingBox(ringModel, Matrix.CreateTranslation(ringPosition)));
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

            _spriteBatch.Begin();
            if(!allowMovement)
            {
                _spriteBatch.Draw(buttonSprite, new Rectangle(0, 0,120,80), Color.White);
            }
            Vector2 scoreTextWidth = font.MeasureString(scoreText);
            _spriteBatch.DrawString(font, scoreText, new Vector2((GraphicsDevice.Viewport.Width/2)-(scoreTextWidth.X/2), 0), Color.Black);
            _spriteBatch.End();

            GraphicsDevice.DepthStencilState = DepthStencilState.Default; //This fixed broken Models with SpriteBatch and 3D Models

            // TODO: Add your drawing code here
            for (int i = 0; i < rings.Count; i++)
            {
                if (rings.ElementAt(i).dead)
                {
                    continue;
                }
                DrawModel(ringModel, Matrix.CreateTranslation(rings.ElementAt(i).position), viewMatrix,projectionMatrix);
            }
            
            DrawModel(spaceShip,/*Matrix.CreateRotationX(MathHelper.ToRadians(camPosition.Z))**/Matrix.CreateTranslation(shipPosition), viewMatrix,projectionMatrix);
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