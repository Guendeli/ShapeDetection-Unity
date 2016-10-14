using UnityEngine;
using System.Collections.Generic;
using ShapeDetector;

public class ShapeBehaviour : MonoBehaviour {

    /// <summary>
    /// Disable or enable gesture recognition
    /// </summary>
    public bool isEnabled = true;

    /// <summary>
    /// Overwrite the XML file in persistent data path
    /// </summary>
    public bool forceCopy = false;

    /// <summary>
    /// Use the faster algorithm, however default (slower) algorithm has a better scoring system
    /// </summary>
    public bool useProtractor = false;

    /// <summary>
    /// The name of the gesture library to load. Do NOT include '.xml'
    /// </summary>
    public string libraryToLoad = "shapes";

    /// <summary>
    /// A new point will be placed if it is this further than the last point.
    /// </summary>
	public float distanceBetweenPoints = 10f;

    /// <summary>
    /// Minimum amount of points required to recognize a gesture.
    /// </summary>
	public int minimumPointsToRecognize = 10;

    /// <summary>
    /// Material for the line renderer.
    /// </summary>
    public Material lineMaterial;

    /// <summary>
    /// Start thickness of the gesture.
    /// </summary>
    public float startThickness = 0.25f;

    /// <summary>
    /// End thickness of the gesture.
    /// </summary>
    public float endThickness = 0.05f;

    /// <summary>
    /// Start color of the gesture.
    /// </summary>
    public Color startColor = new Color(0, 0.67f, 1f);

    /// <summary>
    /// End color of the gesture.
    /// </summary>
    public Color endColor = new Color(0.48f, 0.83f, 1f);

    /// <summary>
    /// Limits gesture drawing to a specific area
    /// </summary>
    public ShapeLimitType shapeLimitType = ShapeLimitType.None;

    /// <summary>
    /// RectTransform to limit gesture
    /// </summary>
    public RectTransform shapeLimitRectBounds;

    /// <summary>
    /// Rect of the gestureLimitRectBounds
    /// </summary>
    Rect shapeLimitRect;

    /// <summary>
    /// Parent canvas of RectTransform to limit gesture.
    /// Set the pivot to bottom-left corner
    /// </summary>
    Canvas parentCanvas;

    /// <summary>
    /// Current platform.
    /// </summary>
    RuntimePlatform platform;

    /// <summary>
    /// Line renderer component. 
    /// </summary>
    LineRenderer shapeRenderer;

    /// <summary>
    /// The position of the point on the screen. 
    /// </summary>
    Vector3 virtualKeyPosition = Vector2.zero;

    /// <summary>
    /// A new point. 
    /// </summary>
    Vector2 point;

    /// <summary>
    /// List of points that form the gesture. 
    /// </summary>
    List<Vector2> points = new List<Vector2>();

    /// <summary>
    /// Vertex count of the line renderer. 
    /// </summary>
    int vertexCount = 0;

    /// <summary>
    /// Loaded gesture library. 
    /// </summary>
    ShapeLibrary gl;

    /// <summary>
    /// Recognized gesture. 
    /// </summary>
    Shape gesture;

    /// <summary>
    /// Result. 
    /// </summary>
    Result result;

    /// <summary>
    /// This is the event to subscribe to.
    /// </summary>
    /// <param name="r">Result of the recognition</param>
    public delegate void ShapeEvent(Result r);
    public static event ShapeEvent OnRecognition;


    // Get the platform and apply attributes to line renderer.
    void Awake() {
        platform = Application.platform;
        shapeRenderer = gameObject.AddComponent<LineRenderer>();
        shapeRenderer.SetVertexCount(0);
        shapeRenderer.material = lineMaterial;
        shapeRenderer.SetColors(startColor, endColor);
        shapeRenderer.SetWidth(startThickness, endThickness);
    }


    // Load the library.
    void Start() {
        gl = new ShapeLibrary(libraryToLoad, forceCopy);

        if (shapeLimitType == ShapeLimitType.RectBoundsClamp) {
            parentCanvas = shapeLimitRectBounds.GetComponentInParent<Canvas>();
            shapeLimitRect = RectTransformUtility.PixelAdjustRect(shapeLimitRectBounds, parentCanvas);
            shapeLimitRect.position += new Vector2(shapeLimitRectBounds.position.x, shapeLimitRectBounds.position.y);
        }
    }


    // Track user input and fire OnRecognition event when necessary.
    void Update() {

        // Track user input if GestureRecognition is enabled.
        if (isEnabled) {

            // If it is a touch device, get the touch position
            // if it is not, get the mouse position
            if (Utility.IsTouchDevice()) {
                if (Input.touchCount > 0) {
                    virtualKeyPosition = new Vector3(Input.GetTouch(0).position.x, Input.GetTouch(0).position.y);
                }
            } else {
                if (Input.GetMouseButton(0)) {
                    virtualKeyPosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y);
                }
            }

            // It is not necessary to track the touch from this point on,
            // because it is already registered, and GetMouseButton event 
            // also fires on touch devices
            if (Input.GetMouseButton(0)) {

                switch (shapeLimitType) {

                    case ShapeLimitType.None:
                        RegisterPoint();
                        break;

                    case ShapeLimitType.RectBoundsIgnore:
                        if (RectTransformUtility.RectangleContainsScreenPoint(shapeLimitRectBounds, virtualKeyPosition, null)) {
                            RegisterPoint();
                        }
                        break;

                    case ShapeLimitType.RectBoundsClamp:
                        virtualKeyPosition = Utility.ClampPointToRect(virtualKeyPosition, shapeLimitRect);
                        RegisterPoint();
                        break;
                }

            }

            // Capture the gesture, recognize it, fire the recognition event,
            // and clear the gesture from the screen.
            if (Input.GetMouseButtonUp(0)) {

                if (points.Count > minimumPointsToRecognize) {
                    gesture = new Shape(points);
                    result = gesture.Recognize(gl, useProtractor);

                    if (OnRecognition != null) {
                        OnRecognition(result);
                    }
                }

                ClearGesture();
            }
        }

    }


    /// <summary>
    /// Register this point only if the point list is empty or current point
    /// is far enough than the last point. This ensures that the gesture looks
    /// good on the screen. Moreover, it is good to not overpopulate the screen
    /// with so much points.
    /// </summary>
    void RegisterPoint() {
        point = new Vector2(virtualKeyPosition.x, -virtualKeyPosition.y);

        if (points.Count == 0 || (points.Count > 0 && Vector2.Distance(point, points[points.Count - 1]) > distanceBetweenPoints)) {
            points.Add(point);

            shapeRenderer.SetVertexCount(++vertexCount);
            shapeRenderer.SetPosition(vertexCount - 1, Utility.WorldCoordinateForGesturePoint(virtualKeyPosition));
        }
    }


    /// <summary>
    /// Remove the gesture from the screen.
    /// </summary>
    void ClearGesture() {
        points.Clear();
        shapeRenderer.SetVertexCount(0);
        vertexCount = 0;
    }
}
