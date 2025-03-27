using System.Collections.Generic;
using App.Scripts.Modules.GridModel;
using App.Scripts.Scenes.SceneHeroes.Features.Grid.LevelInfo.Config;
using UnityEngine;

namespace App.Scripts.Scenes.SceneHeroes.Features.PathFinding
{
    public class ServiceUnitNavigator : IServiceUnitNavigator
    {
        //public List<Vector2Int> FindPath(UnitType unitType, Vector2Int from, Vector2Int to, Grid<int> gridMatrix)
        //{
        //    //implement find path here
        //    return new List<Vector2Int> { from, to};
        //}
        // ����� ��� ������ ����������� ���� �� ������ ���� �����, ��������� � �������� �������
        public List<Vector2Int> FindPath(UnitType unitType, Vector2Int from, Vector2Int to, Grid<int> gridMatrix)
        {
            // �������� �� ������������ �������
            if (!gridMatrix.IsValid(from) || !gridMatrix.IsValid(to))
            {
                return null; // ���� ������� ��� ������, ���������� null
            }

            // ������������� ������� ��� BFS
            Queue<Vector2Int> queue = new Queue<Vector2Int>();
            Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();
            Dictionary<Vector2Int, int> gScore = new Dictionary<Vector2Int, int>();

            queue.Enqueue(from);
            cameFrom[from] = from; // ������ ����
            gScore[from] = 0; // ��������� ���������

            while (queue.Count > 0)
            {
                Vector2Int current = queue.Dequeue();

                // �������� ���������� �������� �������
                if (current == to)
                {
                    return ReconstructPath(cameFrom, current);
                }

                // �������� ��������� �������� ��� ������� ���� �����
                foreach (var neighbor in GetNeighbors(current, unitType, gridMatrix))
                {
                    // ������ ��������� �����������
                    int tentativeGScore = gScore[current] + 1; // ������������, ��� ������ ��� ����� ��������� 1

                    // ��������� ������ ���� ���� � ������ ������
                    if (!gScore.ContainsKey(neighbor) || tentativeGScore < gScore[neighbor])
                    {
                        cameFrom[neighbor] = current;
                        gScore[neighbor] = tentativeGScore;

                        // ��������� ������ � �������
                        if (!queue.Contains(neighbor))
                        {
                            queue.Enqueue(neighbor);
                        }
                    }
                }
            }

            return null; // ���� ���� �� ������
        }

        // ����� ���������� ������ �������� ������� � ����������� �� ���� �����
        private IEnumerable<Vector2Int> GetNeighbors(Vector2Int position, UnitType unitType, Grid<int> gridMatrix)
        {
            List<Vector2Int> neighbors = new List<Vector2Int>();

            // ����������� ��������� ����������� �������� ��� ������� �����
            Vector2Int[] directions;
            switch (unitType)
            {
                case UnitType.SwordMan:
                    directions = new Vector2Int[] { Vector2Int.right, Vector2Int.left, Vector2Int.up, Vector2Int.down };
                    break;
                case UnitType.HorseMan:
                    directions = new Vector2Int[] { Vector2Int.right, Vector2Int.left, Vector2Int.up,
                                                      Vector2Int.down,
                                                      new Vector2Int(1, 1), new Vector2Int(-1, -1),
                                                      new Vector2Int(1, -1), new Vector2Int(-1, 1) };
                    break;
                case UnitType.Angel:
                    directions = new Vector2Int[] { Vector2Int.right, Vector2Int.left, Vector2Int.up, Vector2Int.down,
                                                      new Vector2Int(1, 1), new Vector2Int(-1, -1),
                                                      new Vector2Int(1, -1), new Vector2Int(-1, 1) }; // ����� ����� ���������
                    break;
                case UnitType.Barbarian:
                    directions = new Vector2Int[] { Vector2Int.right, Vector2Int.left, Vector2Int.up, Vector2Int.down,
                                                      new Vector2Int(1, 1), new Vector2Int(-1, -1),
                                                      new Vector2Int(1, -1), new Vector2Int(-1, 1) };
                    break;
                case UnitType.Poor:
                    directions = new Vector2Int[] { Vector2Int.right, Vector2Int.left, Vector2Int.up, Vector2Int.down,
                                                      new Vector2Int(1, 1), new Vector2Int(-1, -1) };
                    break;
                case UnitType.Shaman:
                    directions = new Vector2Int[] { new Vector2Int(2, 1), new Vector2Int(-2, 1),
                                                      new Vector2Int(-2, -1), new Vector2Int(2, -1),
                                                      new Vector2Int(1, 2), new Vector2Int(-1, 2),
                                                      new Vector2Int(-1, -2), new Vector2Int(1, -2) };
                    break;
                default:
                    directions = new Vector2Int[0];
                    break;
            }

            // ��������� ������ ����������� � ��������� �������
            foreach (var direction in directions)
            {
                Vector2Int neighborPos = position + direction;
                // ��������� ���������� ����� � �����������
                if (gridMatrix.IsValid(neighborPos) && IsMovable(gridMatrix[neighborPos]))
                {
                    neighbors.Add(neighborPos);
                }
            }

            return neighbors;
        }

        // ��������, �������� �� ������ ���������� � ����������� �� ���� �����������
        private bool IsMovable(int cellValue)
        {
            // ���������� �����������
            if (cellValue == (int)ObstacleType.None) return true;
            if (cellValue == (int)ObstacleType.Stone) return false; // ������
            if (cellValue == (int)ObstacleType.Water) return false; // ����

            return false; // � ��������� ������ ������� �������������
        }

        // ����� ��� �������������� ����
        private List<Vector2Int> ReconstructPath(Dictionary<Vector2Int, Vector2Int> cameFrom, Vector2Int current)
        {
            List<Vector2Int> totalPath = new List<Vector2Int>() { current };
            while (cameFrom.ContainsKey(current))
            {
                current = cameFrom[current];
                totalPath.Add(current);
            }
            totalPath.Reverse(); // �������� ������� ��� ���������� ������������������
            return totalPath;
        }
    }
}
    
