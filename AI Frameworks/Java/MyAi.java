import java.io.IOException;

public class MyAi extends Client {

    public MyAi() throws IOException, InterruptedException {
        super();
    }

    // Implement your AI here
    // Use these functions to communicate with server:
    //    SetName(name) - Sets your name to appear in the simulator
    //    GetState() - returns the current GameState object
    //    Move(x, y) - accelerate in the given direction
    @Override
    public void RunAi() throws IOException, InterruptedException {
        SetName("StupidAI");

		char[][] map = WaitForMap();

		for (int r = 0; r < map[0].length; r++)
		{
			for (int c = 0; c < map.length; c++)
				System.out.print(map[c][r]);
			
			System.out.println();
		}

        while (Connected()) {
            // Poll the game state
            GameState state = GetState();

            if (state == null) break;

            // Ignore game state and do something random!
            Move((float) Math.random() * 20 - 10, (float) Math.random() * 20 - 10);

            Thread.sleep(500);
        }
		System.out.println("Connection closed");
    }

    public static void main(String[] args) {
        try {
            MyAi ai = new MyAi();
        } catch (Exception e) {
            e.printStackTrace();
        }
    }

}


