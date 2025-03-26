using System.Collections.Generic;
using System.Linq;
using App.Scripts.Scenes.SceneMatrix.Features.FieldInteractions.Model;
using App.Scripts.Scenes.SceneMatrix.Features.FigurePreviewContainer.Model;
using UnityEngine;

namespace App.Scripts.Scenes.SceneMatrix.Features.FieldInteractions.Services
{
   
    public class ServicePackFieldDummy : IServiceFieldPacker
    {
        //public void GeneratePlacements(List<FigurePlacement> outputPlaces, Vector2Int fieldSize, List<ViewModelFigure> figures)
        //{
        //    //тестовая имплементация - перепишите это!
        //    if (figures.Count == 0)
        //    {
        //        return;
        //    }

        //    outputPlaces.Add(new FigurePlacement
        //    {
        //        Id =  figures[0].Id,
        //        Place = new Vector2Int(0, 0)
        //    });
        //}
        public void GeneratePlacements(List<FigurePlacement> outputPlaces, Vector2Int fieldSize, List<ViewModelFigure> figures)
        {
            // Создание поля для занятых клеток
            bool[,] occupiedGrid = new bool[fieldSize.y, fieldSize.x];

            // Перебор всех фигур и их размещение
            foreach (var figure in figures.OrderByDescending(f => f.Grid.Width * f.Grid.Height)) // Упаковка фигур от больших к меньшим
            {
                TryPlaceFigure(figure, occupiedGrid, outputPlaces);
            }
        }

        private void TryPlaceFigure(ViewModelFigure figure, bool[,] occupiedGrid, List<FigurePlacement> outputPlaces)
        {
            int figureHeight = figure.Grid.Height;
            int figureWidth = figure.Grid.Width;

            // Перебор всех возможных позиций для размещения фигуры
            for (int y = 0; y <= occupiedGrid.GetLength(0) - figureHeight; y++)
            {
                for (int x = 0; x <= occupiedGrid.GetLength(1) - figureWidth; x++)
                {
                    // Проверка, можно ли разместить фигуру на текущей позиции
                    if (CanPlaceFigure(figure, occupiedGrid, x, y))
                    {
                        PlaceFigure(figure, occupiedGrid, new Vector2Int(x, y), outputPlaces);
                        return; // После успешного размещения, выходим из метода
                    }
                }
            }
        }

        private bool CanPlaceFigure(ViewModelFigure figure, bool[,] occupiedGrid, int startX, int startY)
        {
            int figureHeight = figure.Grid.Height;
            int figureWidth = figure.Grid.Width;

            // Проверка всех клеток фигуры
            for (int y = 0; y < figureHeight; y++)
            {
                for (int x = 0; x < figureWidth; x++)
                {
                    if (figure.Grid[x, y] && occupiedGrid[startY + y, startX + x]) // Если клетка фигуры занята
                    {
                        return false; // Нельзя разместить фигуру
                    }
                }
            }

            return true; // Место свободно для размещения фигуры
        }

        private void PlaceFigure(ViewModelFigure figure, bool[,] occupiedGrid, Vector2Int position, List<FigurePlacement> outputPlaces)
        {
            int figureHeight = figure.Grid.Height;
            int figureWidth = figure.Grid.Width;

            // Устанавливаем фигуру на поле и заполняем список размещений
            for (int y = 0; y < figureHeight; y++)
            {
                for (int x = 0; x < figureWidth; x++)
                {
                    if (figure.Grid[x, y])
                    {
                        occupiedGrid[position.y + y, position.x + x] = true; // Занимаем клетку
                    }
                }
            }

            // Добавление размещения фигуры
            outputPlaces.Add(new FigurePlacement { Id = figure.Id, Place = position });
        }
    }
}

