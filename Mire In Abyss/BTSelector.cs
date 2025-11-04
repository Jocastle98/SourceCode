using System.Collections.Generic;

public class BTSelector : BTNode
{
    private readonly List<BTNode> mChildren;
    public BTSelector(params BTNode[] children) => mChildren = new List<BTNode>(children);
    public override bool Tick()
    {
        foreach (var child in mChildren)
            if (child.Tick()) return true;
        return false;
    }
}