using System.Linq;
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

                // 获取时间信息
                var startTime = nextHitObject.HitObject.StartTime;
                var endTime = nextHitObject.HitObject.GetEndTime();

                // 检查类型
                var typeName = nextHitObject.GetType().Name;
            }
            //创建发送
            var Data = new RLData();
            Data.mouse = mousePosition;
            _tcpSender.SendAsync(Data);
        }


    }

    private TcpDataSender _tcpSender=new TcpDataSender("127.0.0.1", 64574);
}
}
