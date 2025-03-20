# Snake Game - Console Version

## Overview
This is a **console-based Snake game** implemented in **C#**. The game allows players to control a snake, collect fruits, and grow in size while avoiding self-collision and the game borders. The game is developed using **object-oriented programming (OOP)** principles and provides a structured approach to handling game mechanics.

## Features
- **Classic Snake Mechanics**: The snake moves in four directions and grows by consuming fruits.
- **Dynamic Fruit Spawning**: Fruits spawn randomly, with changing values based on the player's score.
- **Game Over & Restart Functionality**: The game ends if the snake collides with itself or reaches the boundary. The player can restart the game.
- **Console Graphics**: Utilizes ASCII characters to represent the snake, fruit, and game border.
- **Adjustable Speed**: The game updates at a fixed interval to control snake movement speed.
- **Score Tracking**: Displays current score and tracks high scores.
- **User Input Handling**: Supports both `WASD` and `Arrow keys` for movement.

## Installation & Setup
### Prerequisites
Ensure you have the following installed:
- **.NET SDK (C#)**
- **Visual Studio Code or Visual Studio (Recommended)**

### Running the Game
1. **Clone the repository**
   ```sh
   git clone https://github.com/your-repository/snake-game.git
   cd snake-game
   ```
2. **Build the project**
   ```sh
   dotnet build
   ```
3. **Run the game**
   ```sh
   dotnet run
   ```

## Gameplay Instructions
- **Start the Game**: Run the application to start the game.
- **Control the Snake**:
  - Use `WASD` keys or `Arrow Keys` to move.
  - `W` / `Up Arrow` → Move Up
  - `S` / `Down Arrow` → Move Down
  - `A` / `Left Arrow` → Move Left
  - `D` / `Right Arrow` → Move Right
- **Objective**:
  - Collect fruits to grow and increase your score.
  - Avoid colliding with yourself or the game boundary.
  - The game ends if the snake crashes into itself or the edge.
- **Restart**: When the game ends, press `Y` to restart or `N` to quit.

## Code Structure
The project is structured as follows:

- `Game.cs` - Manages the core game loop and logic.
- `Snake.cs` - Handles the snake's movement, growth, and collision detection.
- `Fruit.cs` - Manages fruit spawning and values.
- `Canvas.cs` - Draws the game board and handles rendering.
- `Score.cs` - Tracks the current and high scores.
- `Gamer.cs` - Handles user input and game restart logic.
- `Utility.cs` - Provides helper functions for movement, rendering, and input handling.
- `Settings.cs` - Stores configuration settings (e.g., canvas size, colors, symbols, etc.).

## SQL Leaderboard Setup
To store player scores and manage a leaderboard, a SQL database is used. Below is the schema setup:

### Database Creation & Security Setup
```sql
-- Create the "leaderboard" database
CREATE DATABASE leaderboard;

-- Switch to the "leaderboard" database
USE leaderboard;

-- Setup security
IF NOT EXISTS (SELECT * FROM sys.server_principals WHERE name = 'LeaderboardLogin')
BEGIN
    CREATE LOGIN LeaderboardLogin WITH PASSWORD = 'P@ssw0rd!';
END

IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = 'LeaderboardUser')
BEGIN
    CREATE USER [LeaderboardUser] FOR LOGIN [LeaderboardLogin];
END

EXEC sp_addrolemember 'db_owner', 'LeaderboardUser';
```

### Gamer & Score Tables
```sql
-- Create Gamer Table
CREATE TABLE Gamer (
    gamer_id      INT NOT NULL IDENTITY(1,1),
    gamer_name    VARCHAR(50) NOT NULL,
    password      VARCHAR(100) NOT NULL,
    highest_score INT NOT NULL DEFAULT 0,
    CONSTRAINT gamer_pk PRIMARY KEY (gamer_id),
    CONSTRAINT gamer_nk UNIQUE (gamer_name)
);

-- Create Score Table
CREATE TABLE Score (
    score_id  INT NOT NULL IDENTITY(1,1),
    gamer_id  INT NOT NULL,
    date      DATE NOT NULL DEFAULT GETDATE(),
    score     INT NOT NULL,
    CONSTRAINT score_pk PRIMARY KEY (score_id),
    CONSTRAINT score_fk FOREIGN KEY (gamer_id)
    REFERENCES Gamer(gamer_id)
    ON DELETE CASCADE
    ON UPDATE CASCADE
);
```

### Leaderboard View
```sql
-- Create Leaderboard View Instead of a Table
CREATE VIEW LeaderboardRank AS
SELECT 
    ROW_NUMBER() OVER (ORDER BY s.score DESC) AS rank,
    g.gamer_id,
    g.gamer_name,
    s.score,
    s.date
FROM Score s
JOIN Gamer g ON s.gamer_id = g.gamer_id;
```

This leaderboard system ensures that scores are stored efficiently while allowing for easy ranking retrieval.

## Future Enhancements
- Add game difficulty levels (adjustable speed, obstacles, etc.).
- Implement multiplayer mode.
- Save and load high scores.
- Improve graphical representation using a GUI framework.


