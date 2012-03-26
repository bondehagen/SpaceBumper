import java.io.BufferedReader;
import java.io.IOException;
import java.io.InputStreamReader;
import java.io.PrintWriter;
import java.net.InetSocketAddress;
import java.net.Socket;

public abstract class Client {

    private Socket client;
    private BufferedReader reader;
    private PrintWriter writer;
    private byte[] data = new byte[256];
    private InetSocketAddress endPoint;
    private boolean started = false;
    private int iteration = 0;

    public GameState GetState() {
        try {
            String line = "";
            if (!started) {
                line = reader.readLine();
                while (line == null || !line.equals("START")) {
                    line = reader.readLine();
                }
                started = true;
            }

            System.out.println("GET_STATE");
            writer.println("GET_STATE");
            writer.flush();

            GameState state = new GameState();
            boolean end = false;
            while (!end) {
                line = reader.readLine();
                if (line != null) {
                    String[] msg = line.split("\\s+");
                    String key = msg[0];
                    if (key.equals("BEGIN_STATE")) {
                        state.Iteration = Integer.parseInt(msg[1]);
                        System.out.print("Receiving state for iteration " + state.Iteration);
                    } else if (key.equals("END_STATE")) {
                        System.out.println(" Done");
                        end = true;
                    } else if (key.equals("BUMPERSHIP")) {
                        state.Bumperships.add(
                                new Bumpership(
                                        Float.parseFloat(msg[1]),
                                        Float.parseFloat(msg[2]),
                                        Float.parseFloat(msg[3]),
                                        Float.parseFloat(msg[4]),
                                        Integer.parseInt(msg[5])));

                    } else if (key.equals("STAR")) {
                        state.Stars.add(new Vector(Float.parseFloat(msg[1]), Float.parseFloat(msg[2])));
                    } else if (key.equals("YOU")) {
                        state.MeIndex = Integer.parseInt(msg[1]);
                    }
                }
            }
            return state;
        } catch (Exception e) {
            e.printStackTrace();
			return null;
        }
    }

	public char[][] WaitForMap()
	{
		try {
			char[][] map = null;
			boolean end = false;
			int r = 0;
			while (!end)
			{
				String line = reader.readLine();
				if (line == null)
					continue;

				String[] msg = line.split("\\s+");
				if (msg.length == 0)
					continue;

				if (msg[0].equals("BEGIN_MAP"))
					map = new char[Integer.parseInt(msg[1])][Integer.parseInt(msg[2])];
				else if (msg[0].equals("END_MAP"))
					end = true;
				else if (map != null) {
					char[] characters = line.toCharArray();
					for (int c = 0; c < characters.length; c++)
						map[c][r] = characters[c];
					r++;
				}
			}
			return map;
		} catch (Exception e) {
            e.printStackTrace();
			return null;
        }
	}
	
    public void SetName(String name) {
        System.out.println("NAME " + name);
        writer.println("NAME " + name);
        writer.flush();
    }

    public void Move(float x, float y) throws IOException {
        System.out.println("ACCELERATION " + x + " " + y);
		writer.println("ACCELERATION " + x + " " + y);
        writer.flush();
    }

    protected boolean Connected() {
        return client.isConnected() && !client.isClosed() && !writer.checkError();
    }

    public Client() throws IOException, InterruptedException {
        // Connect to server
        client = new Socket();
        endPoint = new InetSocketAddress("127.0.0.1", 1986);
        client.connect(endPoint);
        reader = new BufferedReader(new InputStreamReader(client.getInputStream()));
        writer = new PrintWriter(client.getOutputStream(), true);

        // Runs the implementation in MyAI.cs
        RunAi();
    }

    public abstract void RunAi() throws IOException, InterruptedException;
}
