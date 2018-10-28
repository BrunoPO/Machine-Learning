using System;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

namespace UnityStandardAssets.Vehicles.Car
{
    [RequireComponent(typeof (CarController))]
    public class CarUserControl : MonoBehaviour
    {

        private String lastChkPointName = "";
        #region IDGenerator
        // Used for unique ID generation
        private static int idGenerator = 0;
        /// <summary>
        /// Returns the next unique id in the sequence.
        /// </summary>
        private static int NextID
        {
            get { return idGenerator++; }
        }
        #endregion

        private const float MAX_CHECKPOINT_DELAY = 7;

        public Boolean UseUserInput = false;
        public Agent Agent
        {
            get;
            set;
        }
        public float CurrentCompletionReward
        {
            get { return Agent.Genotype.Evaluation; }
            set { Agent.Genotype.Evaluation = value; }
        }
        public double[] CurrentControlInputs
        {
            get { return m_Car.CurrentInputs; }
        }

        private CarController m_Car; // the car controller we want to use
        public SpriteRenderer SpriteRenderer;
        private Sensor[] sensors;
        private float timeSinceLastCheckpoint;

        private void Awake()
        {
            // get the car controller
            m_Car = GetComponent<CarController>();
            SpriteRenderer = GetComponent<SpriteRenderer>();
            sensors = GetComponentsInChildren<Sensor>();
        }



        public void Restart()
        {
            m_Car.enabled = true;
            timeSinceLastCheckpoint = 0;

            foreach (Sensor s in sensors)
                s.Show();

            Agent.Reset();
            this.enabled = true;
        }

        // Unity method for normal update
        void Update()
        {
            timeSinceLastCheckpoint += Time.deltaTime;
        }

        // Unity method for physics update
        void FixedUpdate()
        {
            //Get control inputs from Agent
            if (!UseUserInput)
            {
                //Get readings from sensors
                double[] sensorOutput = new double[sensors.Length];
                for (int i = 0; i < sensors.Length; i++) {
                    sensorOutput[i] = sensors[i].Output;
                    //print(sensorOutput[i]);
                }

                double[] controlInputs = Agent.FNN.ProcessInputs(sensorOutput);
                if (controlInputs != null) { 
                    float[] controlInputsFloat = Array.ConvertAll(controlInputs, x => (float)x);
                    m_Car.Move(controlInputsFloat[0], controlInputsFloat[1], controlInputsFloat[1], 0f);
                }

                if (timeSinceLastCheckpoint > MAX_CHECKPOINT_DELAY)
                {
                    Die();
                }
            }
            else
            {

                float h = CrossPlatformInputManager.GetAxis("Horizontal");
                float v = CrossPlatformInputManager.GetAxis("Vertical");
                m_Car.Move(h, v, v, 0f);
            }
        }

        // Makes this car die (making it unmovable and stops the Agent from calculating the controls for the car).
        private void Die()
        {
            print("You Died");

            this.enabled = false;
            //m_Car.Stop();
            m_Car.enabled = false;

            foreach (Sensor s in sensors)
                s.Hide();

            if(Agent != null)
                Agent.Kill();
        }

        public void CheckpointCaptured()//(String chkPointName
        {
            timeSinceLastCheckpoint = 0;
            print("Taked CheckPoint");
            /*if (lastChkPointName != chkPointName) {
                lastChkPointName = chkPointName;
            }*/
        }

        
        public void OnTriggerEnter(Collider other)
        {
            print(other.gameObject.tag);
            if (other.gameObject.tag == "RoadLimit")
            {
                /*CarUserControl controller = other.gameObject.GetComponent<CarUserControl>();
                if(controller!= null)
                {
                    controller.CheckpointCaptured(this.name);
                }*/
                m_Car.Stop();
                this.Die();
            }
        }
    }
}
