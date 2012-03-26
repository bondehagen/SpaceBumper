import java.util.ArrayList;

public class GameState {
    public ArrayList<Bumpership> Bumperships = new ArrayList<Bumpership>();
    public ArrayList<Vector> Stars = new ArrayList<Vector>();
    public int MeIndex;
	public int Iteration;

    public Bumpership Me() {
        return Bumperships.get(MeIndex);
    }
}