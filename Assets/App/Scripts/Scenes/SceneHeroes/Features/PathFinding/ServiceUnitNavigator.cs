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
        // Метод для поиска кратчайшего пути на основе типа юнита, стартовой и конечной позиций
        public List<Vector2Int> FindPath(UnitType unitType, Vector2Int from, Vector2Int to, Grid<int> gridMatrix)
        {
            // Проверка на допустимость позиций
            if (!gridMatrix.IsValid(from) || !gridMatrix.IsValid(to))
            {
                return null; // Если позиции вне границ, возвращаем null
            }

            // Инициализация очереди для BFS
            Queue<Vector2Int> queue = new Queue<Vector2Int>();
            Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();
            Dictionary<Vector2Int, int> gScore = new Dictionary<Vector2Int, int>();

            queue.Enqueue(from);
            cameFrom[from] = from; // Корень пути
            gScore[from] = 0; // Начальная стоимость

            while (queue.Count > 0)
            {
                Vector2Int current = queue.Dequeue();

                // Проверка достижения конечной позиции
                if (current == to)
                {
                    return ReconstructPath(cameFrom, current);
                }

                // Получаем возможные движения для данного типа юнита
                foreach (var neighbor in GetNeighbors(current, unitType, gridMatrix))
                {
                    // Расчет стоимости перемещения
                    int tentativeGScore = gScore[current] + 1; // Предполагаем, что каждый шаг имеет стоимость 1

                    // Проверяем только если путь к соседу короче
                    if (!gScore.ContainsKey(neighbor) || tentativeGScore < gScore[neighbor])
                    {
                        cameFrom[neighbor] = current;
                        gScore[neighbor] = tentativeGScore;

                        // Добавляем соседа в очередь
                        if (!queue.Contains(neighbor))
                        {
                            queue.Enqueue(neighbor);
                        }
                    }
                }
            }

            return null; // Если путь не найден
        }

        // Метод возвращает список соседних позиций в зависимости от типа юнита
        private IEnumerable<Vector2Int> GetNeighbors(Vector2Int position, UnitType unitType, Grid<int> gridMatrix)
        {
            List<Vector2Int> neighbors = new List<Vector2Int>();

            // Определение возможных направлений движения для каждого юнита
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
                                                      new Vector2Int(1, -1), new Vector2Int(-1, 1) }; // Водой можно проходить
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

            // Проверяем каждое направление и добавляем соседей
            foreach (var direction in directions)
            {
                Vector2Int neighborPos = position + direction;
                // Проверяем валидность семьи и препятствий
                if (gridMatrix.IsValid(neighborPos) && IsMovable(gridMatrix[neighborPos]))
                {
                    neighbors.Add(neighborPos);
                }
            }

            return neighbors;
        }

        // Проверка, является ли клетка проходимой в зависимости от типа препятствия
        private bool IsMovable(int cellValue)
        {
            // Определяем препятствия
            if (cellValue == (int)ObstacleType.None) return true;
            if (cellValue == (int)ObstacleType.Stone) return false; // Камень
            if (cellValue == (int)ObstacleType.Water) return false; // Вода

            return false; // В остальном случае считаем непреодолимым
        }

        // Метод для восстановления пути
        private List<Vector2Int> ReconstructPath(Dictionary<Vector2Int, Vector2Int> cameFrom, Vector2Int current)
        {
            List<Vector2Int> totalPath = new List<Vector2Int>() { current };
            while (cameFrom.ContainsKey(current))
            {
                current = cameFrom[current];
                totalPath.Add(current);
            }
            totalPath.Reverse(); // Обратный порядок для правильной последовательности
            return totalPath;
        }
    }
}
    
