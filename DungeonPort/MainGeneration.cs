using System;
using System.Collections.Generic;


namespace DungeonPort
{
    public enum Tile // Used to hold the tiles of the dungeon.
    {
        Unused = ' ',
        Floor = '.',
        Corridor = ',',
        Wall = '#',
        ClosedDoor = '+',
        OpenDoor = '-',
        UpStairs = '<',
        DownStairs = '>',
        player = 'p'
    }
    public enum Direction // Used to hold the directions it can generate.
    {
        North,
        South,
        West,
        East,
        DirectionCount
    }

    class MainGeneration
    {
        #region Vars 
        private int _width, _height;
        private List<char> _tiles; // Holds the tiles
        private List<Rect> _rooms; // Holds the rooms
        private List<Rect> _exits; // Holds the exits
        Random rand = new Random();
        #endregion

        /*----------------Public Methods-----------------------*/

        public MainGeneration(int width, int height) // Constructor
        {
            _width = width;
            _height = height;
            _tiles = new List<char>(width * height);
            fillList();
            _rooms = new List<Rect>();
            _exits = new List<Rect>();
        }

        public void generate(int maxFeatures) // Main Genreation code.
        {
            //Try and place the first room in the center
            if (!makeRoom(_width / 2, _height / 2, (Direction)rand.Next(0, 4), true))
            {
                Console.WriteLine("Unable to place the first room");
                return;
            }

            //Already placed 1 feature (the first room)
            for (int i = 1; i < maxFeatures; ++i)
            {
                if (!createFeature())
                {
                    Console.WriteLine("Unable to place features " + i + ")");
                }
            }

            if (!placeObject(Tile.UpStairs)) // Places the up stairs
            {
                Console.WriteLine("Unable to place up stairs");
            }

            if (!placeObject(Tile.DownStairs)) // Places the down stiars
            {
                Console.WriteLine("Unable to place down stiars");
            }

            for (int i = 0; i < _tiles.Count; i++) // Used to make so the unused tile is desplates as a '.'
            {
                if (_tiles[i] == (char)Tile.Unused)
                    _tiles[i] = '.';
                else if (_tiles[i] == (char)Tile.Floor || _tiles[i] == (char)Tile.Corridor)
                    _tiles[i] = ' ';
            }
        }

        public void print() // Used to print the whole dungeon
        {
            for (int y = 0; y < _height; ++y)
            {
                for (int x = 0; x < _width; ++x)
                    Console.Write(getTile(x, y));

                Console.WriteLine();
            }
        }

        /*----------------Private Methods-----------------------*/

        private char getTile(int x, int y) // Gets a tile at a position
        {
            if (x < 0 || y < 0 || x >= _width || y >= _height)
                return (char)Tile.Unused;
            return _tiles[x + y * _width];
        }

        private void setTile(int x, int y, Tile tile) // Sets the tile at a position
        {
            _tiles[x + y * _width] = (char)tile;
        }

        private bool createFeature() // Used to create featuers
        {
            rand = new Random(new System.DateTime().Millisecond); // Randomise the seed
            for (int i = 0; i < 1000; ++i)
            {
                if (_exits.Count == 0)
                    break;

                int r = rand.Next(0, _exits.Count);
                int x = rand.Next(_exits[r].x, _exits[r].x + _exits[r].width - 1);
                int y = rand.Next(_exits[r].y, _exits[r].y + _exits[r].height - 1);

                for (int j = 0; j < (int)Direction.DirectionCount; ++j)
                {
                    if (createFeature(x, y, (Direction)j))
                    {
                        _exits.RemoveAt(0 + r);
                        return true;
                    }
                }
            }
            return false;
        }

        private bool createFeature(int x, int y, Direction dir) // Creates a closed door / cooridor going in a diraction and at a positon
        {
            const int roomChance = 50;

            int dx = 0;
            int dy = 0;

            if (dir == Direction.North)
                dy = 1;
            else if (dir == Direction.South)
                dy = -1;
            else if (dir == Direction.West)
                dx = 1;
            else if (dir == Direction.East)
                dx = -1;

            if (getTile(x + dx, y + dy) != (char)Tile.Floor && getTile(x + dx, y + dy) != (char)Tile.Corridor)
                return false;

            if (rand.Next(0, 100) < roomChance)
            {
                if (makeRoom(x, y, dir))
                {
                    setTile(x, y, Tile.ClosedDoor);
                    return true;
                }
            }
            else
            {
                if (makeCorridor(x, y, dir))
                {
                    if (getTile(x + dx, y + dy) == (char)Tile.Floor)
                        setTile(x, y, Tile.ClosedDoor);
                    else
                        setTile(x, y, Tile.Corridor);

                    return true;
                }
            }
            return false;
        }

        private bool makeRoom(int x, int y, Direction dir, bool firstRoom = false) // Used to make rooms
        {
            const int minRoomSize = 3;
            const int maxRoomSize = 6;

            Rect room = new Rect();
            Rect temp = new Rect();
            room.width = rand.Next(minRoomSize, maxRoomSize);
            room.height = rand.Next(minRoomSize, maxRoomSize);

            if (dir == Direction.North)
            {
                room.x = x - room.width / 2;
                room.y = y - room.height;
            }
            else if (dir == Direction.South)
            {
                room.x = x - room.width / 2;
                room.y = y + 1;
            }
            else if (dir == Direction.West)
            {
                room.x = x - room.width;
                room.y = y - room.height / 2;
            }
            else if (dir == Direction.East)
            {
                room.x = x + 1;
                room.y = y - room.height / 2;
            }

            if (placeRect(room, Tile.Floor))
            {
                _rooms.Add(room);

                if (dir != Direction.South || firstRoom)
                {
                    temp = new Rect();
                    temp.x = room.x; /**/ temp.y = room.y - 1; /**/ temp.width = room.width; /**/ temp.height = 1;
                    if (firstRoom)
                    {
                        setTile(room.x + 2, room.y + 1, Tile.player);
                    }
                    _exits.Add(temp);
                }
                if (dir != Direction.North || firstRoom)
                {
                    temp = new Rect();
                    temp.x = room.x; /**/ temp.y = room.y + room.height; /**/ temp.width = room.width; /**/ temp.height = 1;

                    _exits.Add(temp);
                }
                if (dir != Direction.East || firstRoom)
                {
                    temp = new Rect();
                    temp.x = room.x - 1; /**/ temp.y = room.y; /**/ temp.width = 1; /**/ temp.height = room.height;

                    _exits.Add(temp);
                }
                if (dir != Direction.West || firstRoom)
                {
                    temp = new Rect();
                    temp.x = room.x + room.width; /**/ temp.y = room.y; /**/ temp.width = 1; /**/ temp.height = room.height;


                    _exits.Add(temp);
                }
                return true;

            }
            return false;
        }

        private bool makeCorridor(int x, int y, Direction dir) // Used to make cooridors
        {
            const int minCorridorLength = 3;
            const int maxCorridorLength = 6;

            Rect corridor = new Rect();
            Rect temp = new Rect();
            corridor.x = x;
            corridor.y = y;

            if (RandomBool())
            {
                corridor.width = rand.Next(minCorridorLength, maxCorridorLength);
                corridor.height = 1;

                if (dir == Direction.North)
                {
                    corridor.y = y - 1;
                    if (RandomBool())
                        corridor.x = x - corridor.width + 1;
                }
                else if (dir == Direction.South)
                {
                    corridor.y = y + 1;

                    if (RandomBool())
                        corridor.x = x - corridor.width + 1;
                }
                else if (dir == Direction.West)
                    corridor.x = x - corridor.width;
                else if (dir == Direction.East)
                    corridor.x = x + 1;
            }
            else
            {
                corridor.width = 1;
                corridor.height = rand.Next(minCorridorLength, maxCorridorLength);

                if (dir == Direction.North)
                    corridor.y = y - corridor.height;
                else if (dir == Direction.South)
                    corridor.y = y + 1;
                else if (dir == Direction.West)
                {
                    corridor.x = x - 1;

                    if (RandomBool())
                        corridor.y = y - corridor.height + 1;
                }
                else if (dir == Direction.East)
                {
                    corridor.x = x + 1;

                    if (RandomBool())
                        corridor.y = y - corridor.height + 1;
                }
            }

            if (placeRect(corridor, Tile.Corridor))
            {
                if (dir != Direction.South && corridor.width != 1)
                {
                    temp = new Rect();
                    temp.x = corridor.x; temp.y = corridor.y - 1; temp.width = corridor.width; temp.height = 1;
                    _exits.Add(temp);
                }
                if (dir != Direction.North & corridor.width != 1)
                {
                    temp = new Rect();
                    temp.x = corridor.x; /**/ temp.y = corridor.y + corridor.height; /**/ temp.width = corridor.width; /**/ temp.height = 1;
                    _exits.Add(temp);
                }
                if (dir != Direction.East && corridor.height != 1)
                {
                    temp = new Rect();
                    temp.x = corridor.x - 1; /**/ temp.y = corridor.y; /**/ temp.width = 1; /**/ temp.height = corridor.height;
                    _exits.Add(temp);
                }
                if (dir != Direction.West && corridor.height != 1)
                {
                    temp = new Rect();
                    temp.x = corridor.x + corridor.width; /**/ temp.y = corridor.y; /**/ temp.width = 1; /**/ temp.height = corridor.height;
                    _exits.Add(temp);
                }
                return true;
            }
            return false;
        }

        private bool placeRect(Rect rect, Tile tile) // This takes in a rectagle of size and stuff and places tiles so it fits the rect
        {
            if (rect.x < 1 || rect.y < 1 || rect.x + rect.width > _width - 1 || rect.y + rect.height > _height - 1)
                return false;

            for (int y = rect.y; y < rect.y + rect.height; ++y)
                for (int x = rect.x; x < rect.x + rect.width; ++x)
                {
                    if (getTile(x, y) != (char)Tile.Unused)
                        return false;
                }

            for (int y = rect.y - 1; y < rect.y + rect.height + 1; ++y)
                for (int x = rect.x - 1; x < rect.x + rect.width + 1; ++x)
                {
                    if (x == rect.x - 1 || y == rect.y - 1 || x == rect.x + rect.width || y == rect.y + rect.height)
                        setTile(x, y, Tile.Wall);
                    else
                        setTile(x, y, tile);
                }

            return true;
        }

        private bool placeObject(Tile tile) // Used to place certian objects like doors.
        {
            if (_rooms.Count == 0)
                return false;

            int r = rand.Next(0, _rooms.Count);
            int x = rand.Next(_rooms[r].x + 1, _rooms[r].x + _rooms[r].width - 2);
            int y = rand.Next(_rooms[r].y + 1, _rooms[r].y + _rooms[r].height - 2);

            if (getTile(x, y) == (char)Tile.Floor)
            {
                setTile(x, y, tile);

                _rooms.RemoveAt(0 + r);

                return true;
            }
            return false;
        }

        /*----------------Other Methods-----------------------*/
        private bool RandomBool() // Used to generate a random bool value
        {
            return new Random().NextDouble() >= .5;
        }

        private void fillList() // Used to init the _tiles to all unused
        {
            int temp = _width * _height;
            for (int i = 0; i < temp; i++)
            {
                _tiles.Add((char)Tile.Unused);
            }
        }
    }

    struct Rect // Rect struct used to make rectangles
    {
        public int x;
        public int y;
        public int width, height;
    };
}