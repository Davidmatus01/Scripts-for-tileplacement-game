public enum Direction { North, East, South, West }
public enum EdgeType { Road, City, Field }

[System.Serializable]
public class Edge
{
    public Direction direction;
    public EdgeType type;
}
