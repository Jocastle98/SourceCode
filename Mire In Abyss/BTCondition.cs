using System;

public class BTCondition : BTNode
{
    private readonly Func<bool> mCondition;
    public BTCondition(Func<bool> condition) => mCondition = condition;
    public override bool Tick() => mCondition();
}