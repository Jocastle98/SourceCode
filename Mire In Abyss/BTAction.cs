using System;

public class BTAction : BTNode
{
    private readonly Action mAction;
    public BTAction(Action action) => mAction = action;
    public override bool Tick()
    {
        mAction();
        return true;
    }
}