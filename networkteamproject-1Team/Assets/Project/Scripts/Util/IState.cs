using UnityEngine;

/// <summary>
/// 상태패턴을 상속받기 위한 인터페이스
/// </summary>
public interface IState
{
    public void Enter();
    public void Update();
    public void Exit();
}
