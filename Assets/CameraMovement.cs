using UnityEngine;

namespace Dwarfageddon
{
    public class CameraMovement : MonoBehaviour
    {
        // Tiles per second
        private const float _maxSpeed = .6f;
        private const float _acceleration = 2f;
        private const float _deceleration = -2f;

        // Max screen dimension in tiles
        public float MaxZoom = 8f;
        public float MinZoom = 64f;

        private const float _zoomSpeed = 16f;
        private float _zoom = 16;

        private Camera _camera;
        private Vector2 _velocity;

        public Vector2 ScenePostion
        {
            get
            {
                return new Vector2(_camera.transform.position.x, _camera.transform.position.y);
            }
            set
            {
                _camera.transform.position = new Vector3(value.x, value.y, -10);
            }
        }

        private void Awake()
        {
            _camera = GetComponent<Camera>();
            _velocity = Vector2.zero;
        }

        private void Update()
        {
            ZoomCamera();
            MoveCamera();
        }
        
        public void MoveBy(Vector2 translation)
        {
            _camera.transform.Translate(translation);
        }

        private void ZoomCamera()
        {
            var zoomDelta = Input.GetAxis("Mouse ScrollWheel");
            _zoom -= zoomDelta *= _zoomSpeed;

            if (_zoom > MinZoom)
                _zoom = MinZoom;
            else if (_zoom < MaxZoom)
                _zoom = MaxZoom;

            _camera.orthographicSize = _zoom / _camera.aspect / 2;
        }

        private void MoveCamera()
        {
            var inputVector = Vector2.zero;

            if (Input.GetKey(KeyCode.UpArrow))
                inputVector += Vector2.up;

            if (Input.GetKey(KeyCode.DownArrow))
                inputVector += Vector2.down;

            if (Input.GetKey(KeyCode.RightArrow))
                inputVector += Vector2.right;

            if (Input.GetKey(KeyCode.LeftArrow))
                inputVector += Vector2.left;

            inputVector = inputVector.normalized;

            if (_velocity.magnitude == 0)
            {
                if (inputVector.magnitude == 0)
                {
                    // Camera is stationary and no input
                    return;
                }
                else
                {
                    // Camera is stationary and input
                    StartMoving(inputVector);
                }
            }
            else
            {
                if (inputVector.magnitude == 0)
                {
                    // Camera is moving and no input
                    Decelerate();
                }
                else
                {
                    // Camera is moving and input
                    Accelerate(inputVector);
                }
            }

            _camera.transform.position += (Vector3)_velocity;
        }

        private void StartMoving(Vector2 inputVector)
        {
            _velocity = inputVector * _acceleration * Time.deltaTime;
        }

        private void Accelerate(Vector2 inputVector)
        {
            var velocityParallelComponent = inputVector * (Vector2.Dot(inputVector, _velocity) / inputVector.magnitude);
            var inputParallelComponent = inputVector * _acceleration * Time.deltaTime;

            Vector2 parallelComponent;

            if (velocityParallelComponent.magnitude + inputParallelComponent.magnitude > _maxSpeed)
            {
                parallelComponent = inputVector * _maxSpeed;
            }
            else
            {
                parallelComponent = velocityParallelComponent + inputParallelComponent;
            }

            var velocityPerpendicularComponent = _velocity - velocityParallelComponent;
            var inputPerpendicularComponent = velocityPerpendicularComponent.normalized * _deceleration * Time.deltaTime;

            Vector2 perpendicularComponent;

            if (velocityPerpendicularComponent.magnitude < inputPerpendicularComponent.magnitude)
            {
                perpendicularComponent = Vector2.zero;
            }
            else
            {
                perpendicularComponent = velocityPerpendicularComponent + inputPerpendicularComponent;
            }

            _velocity = parallelComponent + perpendicularComponent;
        }

        private void Decelerate()
        {
            var velocityDelta = _velocity.normalized * _deceleration * Time.deltaTime;

            if (velocityDelta.magnitude < _velocity.magnitude)
            {
                _velocity = _velocity + velocityDelta;
            }
            else
            {
                _velocity = Vector2.zero;
            }
        }
    }
}
