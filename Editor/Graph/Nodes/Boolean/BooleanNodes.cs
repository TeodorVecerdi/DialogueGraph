namespace DialogueGraph {
    [Title("Boolean", "Not")]
    public class NotBooleanNode : UnaryBooleanNode {
        protected override string Title => "Not/Negate";
        protected override BooleanOperation Operation => BooleanOperation.NOT;
    }

    [Title("Boolean", "And")]
    public class AndBooleanNode : BinaryBooleanNode {
        protected override string Title => "And";
        protected override BooleanOperation Operation => BooleanOperation.AND;
    }

    [Title("Boolean", "Or")]
    public class OrBooleanNode : BinaryBooleanNode {
        protected override string Title => "Or";
        protected override BooleanOperation Operation => BooleanOperation.OR;
    }

    [Title("Boolean", "Xor")]
    public class XorBooleanNode : BinaryBooleanNode {
        protected override string Title => "Xor";
        protected override BooleanOperation Operation => BooleanOperation.XOR;
    }

    [Title("Boolean", "Nand")]
    public class NandBooleanNode : BinaryBooleanNode {
        protected override string Title => "Nand";
        protected override BooleanOperation Operation => BooleanOperation.NAND;
    }

    [Title("Boolean", "Nor")]
    public class NorBooleanNode : BinaryBooleanNode {
        protected override string Title => "Nor";
        protected override BooleanOperation Operation => BooleanOperation.NOR;
    }

    [Title("Boolean", "Xnor")]
    public class XnorBooleanNode : BinaryBooleanNode {
        protected override string Title => "Xnor";
        protected override BooleanOperation Operation => BooleanOperation.XNOR;
    }
}