public class Bumpership {
    public Vector Position;
    public Vector Velocity;
	public int Score;
	
    public Bumpership(float x, float y, float vx, float vy, int score) {
        this.Position = new Vector(x, y);
        this.Velocity = new Vector(vx, vy);
		this.Score = score;
    }
}