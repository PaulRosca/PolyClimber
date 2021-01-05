using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CarController : MonoBehaviour
{
    internal enum driveType
    {
        FWD,
        RWD,
        AWD
    }
    [SerializeField] private driveType drive;
    [SerializeField] private List<float> gears;
    [SerializeField] private List<int> gearSpeed;
    private int gearNumber = 1;
    private float smoothTime = 0.09f;

    private Rigidbody rigidbody;

    public Transform centerOfMass;
    public WheelCollider[] wheels = new WheelCollider[4];
    public Transform[] wheelsMeshes = new Transform[4];
    private float torque;

    public AnimationCurve torqueGraph;


    public float downForce = 50;

    public float finalDrive;
    // public float transmissionEfficency;

    private int speed;
    private int wheelsRPM;
    private int engineRPM;

    public GameObject d_wheelRPM;
    public GameObject speedometer;
    public GameObject gearDisplay;
    public GameObject d_engineRPM;

    private bool accelerating=false;
    private bool reversing = false;

    private void Start()
    {
        rigidbody = this.GetComponent<Rigidbody>();
        rigidbody.centerOfMass = centerOfMass.localPosition;
        Input.gyro.enabled = true;
    }
    private void display()
    {
        d_wheelRPM.GetComponent<Text>().text = "Wheels rpm : " + wheelsRPM.ToString();
        speedometer.GetComponent<Text>().text = "Speed : " + speed.ToString() + "KM/H";
        string g;
        if (gearNumber == 0)
            g = "R";
        else
            g = gearNumber.ToString();
        gearDisplay.GetComponent<Text>().text = "Gear : " + g;
        d_engineRPM.GetComponent<Text>().text = "RPM : " + engineRPM;
    }
    private void updateMeshes()
    {
        Vector3 position = Vector3.zero;
        Quaternion rotation = Quaternion.identity;
        for (int i = 0; i < wheels.Length; i++)
        {
            wheels[i].GetWorldPose(out position, out rotation);
            wheelsMeshes[i].position = position;
            wheelsMeshes[i].rotation = rotation;
        }
    }
    private void tractionControl()
    {
        if (Mathf.Abs(wheelsRPM) > 5000)
            torque = 0;
    }
    private void apply_acceleration()
    {
        tractionControl();
        print(torque);
        if (drive == driveType.AWD)
            for (int i = 0; i < 4; i++)
                wheels[i].motorTorque = torque / 4;
        else if (drive == driveType.RWD)
            for (int i = 0; i < 2; i++)
                wheels[i].motorTorque = torque / 2;
        else
            for (int i = 2; i < 4; i++)
                wheels[i].motorTorque = torque / 2;
    }
    public void accelerate()
    {
        if (gearNumber == 0)
            gearNumber = 1;
        accelerating = true;
    }
    public void s_accelerating()
    {
        accelerating = false;
        for (int i = 0; i < wheels.Length; i++)
            wheels[i].motorTorque = 0;
    }
    public void brake()
    {
        if (gearNumber == 0)
            reversing = true;
        else
            for (int i = 0; i < wheels.Length; i++)
                wheels[i].brakeTorque = 1000;
    }
    public void s_braking()
    {
        
        for (int i = 0; i < wheels.Length; i++)
        {
            wheels[i].brakeTorque = 0;
            if(reversing)
                wheels[i].motorTorque = 0;
        }
        reversing = false;
    }
    private void steering()
    {
        float giro = Input.gyro.gravity.x*100;
        if (giro > 50)
            giro = 50;
        else if (giro < -50)
            giro = -50;
        if (giro < 2 && giro > -2)// Deadzone
            giro = 0;
        giro /= 50;
        for (int i = 0; i < 2; i++)
            wheels[i].steerAngle = 33 * giro;
    }
    private void calculateWheelsRPM()
    {
        wheelsRPM = 0;
        for (int i = 0; i < wheels.Length; i++)
            wheelsRPM += (int)wheels[i].rpm;
        wheelsRPM = (int)(wheelsRPM / 4 * finalDrive * gears[gearNumber]);
    }
    private void calculateSpeed()
    {
        speed = (int)(this.GetComponent<Rigidbody>().velocity.magnitude * 3.6f);
    }

    private void addDownForce()
    {
        rigidbody.AddForce(-transform.up * downForce * rigidbody.velocity.magnitude);
    }
    private void calculateEngineSpeed()
    {
        torque =  torqueGraph.Evaluate(engineRPM) * finalDrive * gears[gearNumber];
        if (Mathf.Abs(wheelsRPM) < 1000)
            engineRPM = 1500;
        else
            engineRPM = Mathf.Abs(wheelsRPM);
    }
    private void gearChanging()
    {
        if (gearNumber == 0 && engineRPM > 2000)
            torque = 0;
        else if (engineRPM >= 4000 && speed>=gearSpeed[gearNumber] && gearNumber < gears.Count)
            gearNumber++;
        else if (wheelsRPM < 2000 && gearNumber > 1)
            gearNumber--;
    }
    private void reverse()
    {
        if (speed <= 0 && wheels[0].brakeTorque > 0)
        {
            s_braking();
            reversing = true;
            gearNumber = 0;
        }
    }
    private void FixedUpdate()
    {
        calculateWheelsRPM();
        calculateEngineSpeed();
        calculateSpeed();
        steering();
        updateMeshes();
        display();
        addDownForce();
        gearChanging();
        if (accelerating || reversing)
            apply_acceleration();
        reverse();
    }
}
