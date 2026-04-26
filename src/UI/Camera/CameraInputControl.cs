using Godot;

namespace SRC.UI.Camera
{
    public partial class CameraInputControl : Node3D
    {
        [Export] public float RotationSpeed = 5.0f;    // 旋转灵敏度（度/像素）
        [Export] public float ZoomSpeed = 2.0f;        // 缩放速度
        [Export] public float SmoothSpeed = 10.0f;     // 平滑系数（0~20，0=无平滑）

        [Export] public float MinDistance = 0.5f;
        [Export] public float MaxDistance = 15.0f;
        [Export] public float PitchMin = -30.0f;       // 最小俯角（度）
        [Export] public float PitchMax = 80.0f;        // 最大俯角

        private Camera3D _camera;

        // 轨道参数（球坐标）
        private float _targetYaw = 0f;          // 水平角（度）
        private float _targetPitch = 30f;       // 俯仰角（度）
        private float _targetDistance = 5f;     // 距离

        private float _currentYaw;
        private float _currentPitch;
        private float _currentDistance;

        private bool _isDragging = false;
        private Vector2 _lastMousePos;

        public override void _Ready()
        {
            _camera = GetNode<Camera3D>("Camera3D");
            if (_camera == null)
            {
                GD.PrintErr("轨道相机: 未找到 Camera3D 子节点");
                return;
            }

            // 根据当前相机位置反算出初始轨道参数（使编辑器摆放的视角有效）
            Vector3 offset = _camera.Position;          // 相机相对于父节点的局部坐标
            _targetDistance = offset.Length();
            _targetDistance = Mathf.Clamp(_targetDistance, MinDistance, MaxDistance);

            // 计算俯仰角（注意：Godot Y轴向上，俯仰角为offset与XZ平面的夹角）
            _targetPitch = Mathf.RadToDeg(Mathf.Asin(offset.Y / _targetDistance));
            _targetPitch = Mathf.Clamp(_targetPitch, PitchMin, PitchMax);

            // 计算偏航角（从XZ平面角度）
            _targetYaw = Mathf.RadToDeg(Mathf.Atan2(offset.X, offset.Z));
            // 确保角度范围 -180..180
            if (_targetYaw < -180) _targetYaw += 360;
            if (_targetYaw > 180) _targetYaw -= 360;

            // 同步当前值
            _currentYaw = _targetYaw;
            _currentPitch = _targetPitch;
            _currentDistance = _targetDistance;

            // 立即更新相机位置
            UpdateCameraPosition();
        }

        public override void _Input(InputEvent @event)
        {
            // 左键拖拽旋转
            if (@event is InputEventMouseButton mouseEvent && mouseEvent.ButtonIndex == MouseButton.Left)
            {
                if (mouseEvent.Pressed)
                {
                    _isDragging = true;
                    Input.MouseMode = Input.MouseModeEnum.Captured;
                    _lastMousePos = mouseEvent.Position;
                }
                else
                {
                    _isDragging = false;
                    Input.MouseMode = Input.MouseModeEnum.Visible;
                }
            }

            // 鼠标拖动时改变目标角度
            if (@event is InputEventMouseMotion motionEvent && _isDragging)
            {
                Vector2 delta = motionEvent.Relative;
                _targetYaw -= delta.X * RotationSpeed * 0.01f;
                _targetPitch += delta.Y * RotationSpeed * 0.01f;
                _targetPitch = Mathf.Clamp(_targetPitch, PitchMin, PitchMax);
                // 使偏航角保持在合理范围内，避免无限累加
                //if (_targetYaw > 360) _targetYaw -= 360;
                //if (_targetYaw < -360) _targetYaw += 360;
            }

            // 滚轮缩放
            if (@event is InputEventMouseButton wheelEvent)
            {
                if (wheelEvent.ButtonIndex == MouseButton.WheelUp)
                    _targetDistance -= ZoomSpeed * 0.1f;
                else if (wheelEvent.ButtonIndex == MouseButton.WheelDown)
                    _targetDistance += ZoomSpeed * 0.1f;
                _targetDistance = Mathf.Clamp(_targetDistance, MinDistance, MaxDistance);
            }
        }

        public override void _Process(double delta)
        {
            float dt = (float)delta;

            // 平滑过渡（若SmoothSpeed为0则直接赋值）
            if (SmoothSpeed > 0)
            {
                _currentYaw = Mathf.Lerp(_currentYaw, _targetYaw, SmoothSpeed * dt);
                _currentPitch = Mathf.Lerp(_currentPitch, _targetPitch, SmoothSpeed * dt);
                _currentDistance = Mathf.Lerp(_currentDistance, _targetDistance, SmoothSpeed * dt);
            }
            else
            {
                _currentYaw = _targetYaw;
                _currentPitch = _targetPitch;
                _currentDistance = _targetDistance;
            }

            UpdateCameraPosition();
        }

        private void UpdateCameraPosition()
        {
            if (_camera == null) return;

            // 球坐标转笛卡尔坐标（局部坐标，Y轴向上）
            float yawRad = Mathf.DegToRad(_currentYaw);
            float pitchRad = Mathf.DegToRad(_currentPitch);

            float x = Mathf.Cos(pitchRad) * Mathf.Sin(yawRad);
            float z = Mathf.Cos(pitchRad) * Mathf.Cos(yawRad);
            float y = Mathf.Sin(pitchRad);

            Vector3 direction = new Vector3(x, y, z).Normalized();
            Vector3 newPosition = direction * _currentDistance;

            _camera.Position = newPosition;
            // 让相机始终注视父节点（轨道中心）
            _camera.LookAt(Vector3.Zero, Vector3.Up);
        }
    }
}