using System.Linq;
using Newtonsoft.Json.Linq;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Play;

namespace osu.Game.RL
{
    public partial class CustomPlayer : SoloPlayer
    {
        // 暴露 ScoreProcessor 用于访问分数
        public new ScoreProcessor ScoreProcessor => base.ScoreProcessor;

        // 暴露 DrawableRuleset 用于访问 hit objects
        public new DrawableRuleset DrawableRuleset => base.DrawableRuleset;

        protected override void Update()
        {
            base.Update();

            // 获取当前鼠标位置
            var mousePosition = GetContainingInputManager()?.CurrentState.Mouse.Position;

            JObject json = new JObject();

            if (mousePosition.HasValue)
            {
                json["mouse"] = new JArray() { mousePosition.Value.X, mousePosition.Value.Y };
            }

            // 获取血量
            if (HealthProcessor != null)
            {
                json["health"] = HealthProcessor.Health.Value;
            }

            // 获取最近的 hit objects
            if (DrawableRuleset?.Playfield != null)
            {
                var currentTime = DrawableRuleset.FrameStableClock.CurrentTime;

                // 获取下一个未判定的 hit object
                var nextHitObject = DrawableRuleset.Playfield.HitObjectContainer.AliveObjects
                                                   .Where(obj => !obj.Judged && obj.HitObject.StartTime >= currentTime)
                                                   .OrderBy(obj => obj.HitObject.StartTime)
                                                   .FirstOrDefault();

                if (nextHitObject != null)
                {
                    // 获取 hit object 的位置
                    var position = nextHitObject.ToScreenSpace(nextHitObject.OriginPosition);

                    // 获取相对于当前时间的 dt
                    var dt = nextHitObject.HitObject.StartTime - currentTime;
                    
                    // 检查类型
                    var typeName = nextHitObject.HitObject.GetType().Name;
                    string type = "unknown";
                    if (typeName == "HitCircle")
                        type = "circle";
                    else if (typeName == "Slider")
                        type = "slider";

                    json["nextHit"] = new JObject
                    {
                        ["x"] = position.X,
                        ["y"] = position.Y,
                        ["dt"] = dt,
                        ["type"] = type
                    };
                }
            }

            _tcpSender.SendAsync(json.ToString(Newtonsoft.Json.Formatting.None));
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            _tcpSender?.Dispose();
        }

        private TcpDataSender _tcpSender = new TcpDataSender("127.0.0.1", 64574);
    }
}
