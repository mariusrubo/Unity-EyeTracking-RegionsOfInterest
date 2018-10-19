using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EyeTracking : MonoBehaviour {


    [SerializeField]
    Camera cam; // the VR camera

    [SerializeField]
    Transform Roi1;
    ROI roi1;

    [SerializeField]
    Transform Roi2;
    ROI roi2;
    

    // Use this for initialization
    void Start () {
        roi1 = new ROI(Roi1, "ConspecificHead");
        roi2 = new ROI(Roi2, "RedFlowerPot");
    }

	
	// Update is called once per frame
	void Update () {
        
        Ray lGaze = MyEyeTracker.leftGaze(); // here your eye-tracker needs to provide a Ray consisting of the position and direction vector from the left eye
        Ray rGaze = MyEyeTracker.rightGaze();

        roi1 = ProcessROI(roi1, cam, lGaze, rGaze); // this processes all the data relevant for this ROI. 
        roi2 = ProcessROI(roi2, cam, lGaze, rGaze);

    }


    public class ROI
    {
        public Transform transform;
        public string name;
        public Renderer renderer;
        public bool onCam; // whether or not visible from the camera
        public bool inLineOfSight; // whether view from camera to Roi is occluded by something else
        public float distance; // distance to camera
        public float angleToRoi; // angle from camera to the roi
        public float angleToRoiLeft; // angle from left eye to the roi
        public float angleToRoiRight; // angle from right eye to the roi
        public float angleFromRoi; // the other way round: assuming Roi has a "forward", this is the angle at which the camera appears from the roi's perspective (i.e. if the roi is looking into the camera)
        public Vector3 pointOnCam; // the camera captures a 2D representation of the scene. Where on this image is the middle of the roi?

        // get deviations of angle in x and y axis separately. This is important for real-time drift correction (if, for instance, the eye-tracker reliably mislocates gaze of one eye 2 degrees too far downwards)
        public GameObject ReferenceLeft; // will be a hypothetical left eye looking directly at the roi
        public GameObject ReferenceRight;
        public GameObject lEye; // will be a copy of the eye as it is currently tracked, but as a child of the ReferenceLeft, so that we can separate angle deviations along individual axes in space
        public GameObject rEye;
        public float angleFromRoiLeftX; 
        public float angleFromRoiLeftY; 
        public float angleFromRoiRightX; 
        public float angleFromRoiRightY; 

        // to initialize roi
        public ROI(Transform obj, string title)
        {
            transform = obj;
            name = title;
            renderer = obj.GetComponent<Renderer>();
            onCam = false;
            inLineOfSight = false;
            distance = 999;
            angleToRoi = 999;
            angleToRoiLeft = 999;
            angleToRoiRight = 999;
            angleFromRoi = 999;
            pointOnCam = new Vector2(999, 999);

            ReferenceLeft = new GameObject();
            ReferenceRight = new GameObject();
            lEye = new GameObject();
            rEye = new GameObject();
            lEye.transform.parent = ReferenceLeft.transform;
            rEye.transform.parent = ReferenceRight.transform;
        }
    }

    // this processes the roi, and updates all the internal variables
    // leftGaze and rightGaze must be Vectors in world space, not local space! 
    //public ROI ProcessROI(ROI roi, Camera cam, Vector3 leftGaze, Vector3 rightGaze)
    public ROI ProcessROI(ROI roi, Camera cam, Ray leftGaze, Ray rightGaze)
    {
        // check if on Cam
        if (roi.renderer.IsVisibleFrom(cam)) { roi.onCam = true; }
        else roi.onCam = false;

        // check inlineOfSight
        RaycastHit hit;
        Vector3 direction = cam.transform.position - roi.transform.position;
        if (Physics.Raycast(roi.transform.position, direction, out hit)) // then check if it is not occluded by other collider
        {
            if (hit.collider == cam.GetComponent<Collider>()) { roi.inLineOfSight = true; } // if the hit is the camera, there's nothing in between
        }
        else roi.inLineOfSight = false;

        // get distance
        roi.distance = Vector3.Distance(cam.transform.position, roi.transform.position);

        // get angles
        roi.angleToRoi = Vector3.Angle(cam.transform.forward, roi.transform.position - cam.transform.position);
        roi.angleToRoiRight = Vector3.Angle(rightGaze.direction, roi.transform.position - rightGaze.origin);
        roi.angleToRoiLeft = Vector3.Angle(leftGaze.direction, roi.transform.position - leftGaze.origin);
        roi.angleFromRoi = Vector3.Angle(roi.transform.forward, cam.transform.position - roi.transform.position);

        // point on camera texture
        roi.pointOnCam = cam.WorldToScreenPoint(roi.transform.position); // "Screenspace is defined in pixels. The bottom-left of the screen is (0,0); the right-top is (pixelWidth,pixelHeight). The z position is in world units from the camera."

        // get angles in x and y axis
        roi.ReferenceLeft.transform.position = leftGaze.origin;
        roi.ReferenceLeft.transform.LookAt(roi.transform.position, Vector3.up); // rotate hypothetical eyes to look directly at the ROI
        roi.ReferenceRight.transform.position = rightGaze.origin;
        roi.ReferenceRight.transform.LookAt(roi.transform.position, Vector3.up); // rotate hypothetical eyes to look directly at the ROI
        roi.lEye.transform.LookAt(roi.lEye.transform.position + LeftGaze.direction, Vector3.up); // and rotate these eyes to model the eyes of the user as tracked by the eye-tracker
        roi.rEye.transform.LookAt(roi.rEye.transform.position + RightGaze.direction, Vector3.up);
        roi.angleFromRoiLeftX = roi.lEye.transform.localEulerAngles.x; // localeulerangles now let us separate angle in x and y dimension. Note that rotation in y axis gives us left/right deviation
        roi.angleFromRoiLeftY = roi.lEye.transform.localEulerAngles.y;
        roi.angleFromRoiRightX = roi.rEye.transform.localEulerAngles.x;
        roi.angleFromRoiRightY = roi.rEye.transform.localEulerAngles.y;

        return roi;
    }

}
