using System;
using System.Collections.Generic;
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

        public Boolean Commented = false;
        private const float MAX_CHECKPOINT_DELAY = 8;

        public Boolean useSensorObstacle = false;

        public int sensorToBeRead = 6;

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
        private Rigidbody m_Rigidbody;

        private void Awake()
        {
            m_Rigidbody = this.GetComponent<Rigidbody>();
            // get the car controller
            m_Car = GetComponent<CarController>();
            SpriteRenderer = GetComponent<SpriteRenderer>();
            sensors = GetComponentsInChildren<Sensor>();
            sensorToBeRead = (sensorToBeRead > sensors.Length) ? sensors.Length : sensorToBeRead;
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
            //print(m_Rigidbody.velocity);
            timeSinceLastCheckpoint += Time.deltaTime;
        }

        // Unity method for physics update
        void FixedUpdate()
        {
            //Get control inputs from Agent
            if (!UseUserInput)
            {
                //Get readings from sensors
                double[] sensorOutput = null;

                //print(useSensorObstacle);
                if (useSensorObstacle) { 
                    sensorOutput = new double[sensorToBeRead * 2 + 3];
                }
                else
                {
                    sensorOutput = new double[sensorToBeRead + 3];
                }

                float x = m_Rigidbody.velocity.normalized.x;
                x =(x<30)?x/60:0.5f;
                x = (m_Rigidbody.velocity.x < 0) ?x:x+0.5f;
                float y = m_Rigidbody.velocity.normalized.y;
                y = (y < 30) ? y / 60 : 0.5f;
                y = (m_Rigidbody.velocity.y < 0) ? y : y + 0.5f;
                float z = m_Rigidbody.velocity.normalized.z;
                z = (z < 30) ? z / 60 : 0.5f;
                z = (m_Rigidbody.velocity.z < 0) ? z : z + 0.5f;

                sensorOutput[sensorOutput.Length - 3] = x;
                sensorOutput[sensorOutput.Length - 2] = y;
                sensorOutput[sensorOutput.Length-1] = z;

                for (int i = 0; i < sensorToBeRead; i++)
                {
                    sensorOutput[i] = sensors[i].Output.x;
                    //if(Commented) print(sensors[i].Output.x);
                    //print(sensorOutput[i]);
                    if (useSensorObstacle)
                    {
                        sensorOutput[sensorToBeRead+ i] = sensors[i].Output.y;
                    }

                    //print(sensorOutput[i]);
                }



                double[] controlInputs = Agent.FNN.ProcessInputs(sensorOutput);
                if (controlInputs != null) { 
                    float[] controlInputsFloat = Array.ConvertAll(controlInputs, input => (float)input);
                    //controlInputsFloat[0] = (controlInputsFloat[0] * 2) - 1;
                    //controlInputsFloat[1] = (controlInputsFloat[1] * 2) - 1;
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

            TrackManager.Instance.BestCarAll = new KeyValuePair<float, Genotype>(Agent.Genotype.Evaluation, Agent.Genotype);
            //if(Commented) print("You Died");

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
            //if(Commented) print("Taked CheckPoint");
            /*if (lastChkPointName != chkPointName) {
                lastChkPointName = chkPointName;
            }*/
        }

        
        public void OnTriggerEnter(Collider other)
        {
            //print(other.gameObject.tag);
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
