using System.Collections.Generic;

public class BTSequence : BTNode
{
    private readonly List<BTNode> mChildren;
    public BTSequence(params BTNode[] children) => mChildren = new List<BTNode>(children);
    public override bool Tick()
    {
        foreach (var child in mChildren)
            if (!child.Tick()) return false;
        return true;
    }
}