using UnityEngine;

public interface IEnemyAction
{
    //行動の優先順位
    int Priolity { get; }

    //行動を実行できるか
    bool CanExecute();

    //行動開始
    void Enter();

    //行動中
    void Execute();

    //行動終了
    void Exit();
}