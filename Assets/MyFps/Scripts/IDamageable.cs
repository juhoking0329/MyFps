namespace MyFps
{
    /// <summary>
    /// 데미지를 받을 수 있는 오브젝트의 인터페이스
    /// </summary>
    public interface IDamageable
    {
        //데미지를 받는 함수
        void TakeDamage(float damage);
    }
}