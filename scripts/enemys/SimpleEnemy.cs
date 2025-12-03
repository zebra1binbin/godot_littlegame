using Godot;
using System.Threading.Tasks;

public partial class SimpleEnemy : BaseEnemy
{
    public override async void TakeDamage(int dmg)
    {
        AudioManager.Instance.Play(SoundType.EnemyHitSound);
        await Die(); // 直接死亡
    }
}
