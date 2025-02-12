﻿using GasStationMs.App.Forms;
using GasStationMs.App.Modeling.Models.PictureBoxes;
using GasStationMs.App.Modeling.Models.Views;
using System.Drawing;
using System.Windows.Forms;

namespace GasStationMs.App.Modeling.MovingLogic.Car
{
    internal static class CarRouter
    {
        private static ModelingForm _modelingForm;

        internal static void SetUpCarRouter(ModelingForm modelingForm)
        {
            _modelingForm = modelingForm;
        }

        internal static void RouteCar(MoveablePictureBox car)
        {
            if (!car.IsGoesFilling)
            {
                return;
            }

            CarView carView = null;
            CollectorView collectorView = null;

            if (car is CarPictureBox)
            {
                carView = car.Tag as CarView;
            }

            if (car is CollectorPictureBox)
            {
                collectorView = car.Tag as CollectorView;
            }

            var isOnStation = car.IsOnStation;
            var isFilled = car.IsFilled;

            // New car
            if (!isOnStation && !isFilled && !car.HasDestPoints())
            {
                GoToEnter(car);
            }

            // Just entered the station
            if (car is CarPictureBox carPictureBox)
            {
                if (isOnStation && !carView.IsFuelDispenserChosen)
                {
                    ChooseFuelDispenser(carPictureBox);
                }
            }

            if (car is CollectorPictureBox collector)
            {
                if (isOnStation && !collectorView.IsGoesToCashCounter)
                {
                    GoToCashCounter(collector);
                }
            }

            // After filling 
            if (isOnStation && isFilled)
            {
                GoToExit(car);
                car.IsOnStation = false;
            }
        }

        private static void ChooseFuelDispenser(CarPictureBox car)
        {
            var carView = car.Tag as CarView;

            PictureBox optimalFuelDispenser = ModelingProcessor.FuelDispensersList[0];
            FuelDispenserView fuelDispenserView = (FuelDispenserView)optimalFuelDispenser.Tag;
            var minQueue = fuelDispenserView.CarsInQueue;

            // Looking for Fuel Dispenser with minimal queue
            foreach (var fuelDispenser in ModelingProcessor.FuelDispensersList)
            {
                fuelDispenserView = (FuelDispenserView)fuelDispenser.Tag;
                if (fuelDispenserView.CarsInQueue < minQueue)
                {
                    minQueue = fuelDispenserView.CarsInQueue;
                    optimalFuelDispenser = fuelDispenser;
                }
            }

            carView.ChosenFuelDispenser = optimalFuelDispenser;
            fuelDispenserView = (FuelDispenserView)optimalFuelDispenser.Tag;
            fuelDispenserView.CarsInQueue++;
            carView.IsFuelDispenserChosen = true;

            int destPointX;
            int destPointY;
            Point destPoint;

            // Additional points for better graphics
            destPointX = optimalFuelDispenser.Left - DestinationPointsDefiner.FuelingPointDeltaX;
            destPointY = optimalFuelDispenser.Bottom + ElementSizeDefiner.TopologyCellSize - 10;
            destPoint = new Point(destPointX, destPointY);

            car.AddDestinationPoint(destPoint);

            destPointX = optimalFuelDispenser.Left + DestinationPointsDefiner.FuelingPointDeltaX;
            destPointY = optimalFuelDispenser.Bottom + DestinationPointsDefiner.FuelingPointDeltaY;
            destPoint = new Point(destPointX, destPointY);

            // The main point of fueling
            car.AddDestinationPoint(destPoint);

            // Additional points for better graphics
            destPointX = optimalFuelDispenser.Right /*- TopologyCellSize / 2*/;
            destPoint = new Point(destPointX, destPointY + ElementSizeDefiner.CarHeight + 5);
            car.AddDestinationPoint(destPoint);
        }

        private static void GoToCashCounter(CollectorPictureBox collector)
        {
            var collectorView = collector.Tag as CollectorView;
            var cashCounter = ModelingProcessor.CashCounter;

            collectorView.CashCounter = cashCounter;

            collectorView.IsGoesToCashCounter = true;

            int destPointX;
            int destPointY;
            Point destPoint;

            // Additional points for better graphics
            destPointX = cashCounter.Left - ElementSizeDefiner.CarWidth + 5;
            destPointY = cashCounter.Bottom + ElementSizeDefiner.TopologyCellSize - 10;
            destPoint = new Point(destPointX, destPointY);

            collector.AddDestinationPoint(destPoint);

            destPointX = cashCounter.Left + DestinationPointsDefiner.FuelingPointDeltaX;
            destPointY = cashCounter.Bottom + DestinationPointsDefiner.FuelingPointDeltaY;
            destPoint = new Point(destPointX, destPointY);

            // The main point of fueling
            collector.AddDestinationPoint(destPoint);

            // Additional points for better graphics
            destPointX = cashCounter.Right - ElementSizeDefiner.TopologyCellSize / 2;
            destPoint = new Point(destPointX, destPointY + ElementSizeDefiner.CarHeight + 5);
            collector.AddDestinationPoint(destPoint);
        }

        private static void GoToEnter(MoveablePictureBox car)
        {
            car.AddDestinationPoint(DestinationPointsDefiner.EnterPoint3);
            car.AddDestinationPoint(DestinationPointsDefiner.EnterPoint2);
            car.AddDestinationPoint(DestinationPointsDefiner.EnterPoint1);
        }

        private static void GoToExit(MoveablePictureBox car)
        {
            var fillingFinishedPoint = car.GetDestinationPoint();
            car.RemoveDestinationPoint(_modelingForm);

            car.AddDestinationPoint(DestinationPointsDefiner.LeavePointFilled);
            car.AddDestinationPoint(DestinationPointsDefiner.ExitPoint3);
            car.AddDestinationPoint(DestinationPointsDefiner.ExitPoint2);
            car.AddDestinationPoint(DestinationPointsDefiner.ExitPoint1);

            car.AddDestinationPoint(fillingFinishedPoint);
        }

        internal static Point PreventIntersection(MoveablePictureBox activeCar, DirectionEnum.Direction direction)
        {
            var destPoint = activeCar.GetDestinationPoint();

            foreach (Control c in _modelingForm.PlaygroundPanel.Controls)
            {
                if (!(c is PictureBox) || c.Tag == null || c == activeCar)
                {
                    continue;
                }

                var pictureBox = (PictureBox)c;

                if (!activeCar.Bounds.IntersectsWith(pictureBox.Bounds))
                {
                    continue;
                }

                // Another Car
                if (pictureBox is MoveablePictureBox)
                {
                    var anotherCar = pictureBox;

                    switch (direction)
                    {
                        case DirectionEnum.Direction.Up:
                            {
                                activeCar.Top = anotherCar.Bottom;

                                break;
                            }

                        case DirectionEnum.Direction.Right:
                            {
                                activeCar.Left = anotherCar.Left - activeCar.Width;

                                break;
                            }

                        case DirectionEnum.Direction.Down:
                            {
                                activeCar.Top = anotherCar.Top - activeCar.Height;

                                break;
                            }

                        case DirectionEnum.Direction.Left:
                            {
                                activeCar.Left = anotherCar.Right;

                                break;
                            }
                    }
                }


                // Fuel Dispenser
                if (pictureBox.Tag is FuelDispenserView || pictureBox.Tag is CashCounterView)
                {
                    var fuelDispenser = pictureBox;

                    var initialDestinationPoint = activeCar.GetDestinationPoint();

                    bool bypassFromLeft = false;
                    bool bypassFromRight = false;
                    bool bypassFromBottom = false;
                    bool bypassFromTop = false;

                    int newDestX;
                    int newDestY;

                    Point newDestinationPoint1;
                    Point newDestinationPoint2;
                    Point newDestinationPoint3;

                    switch (direction)
                    {
                        case DirectionEnum.Direction.Up:
                            {
                                activeCar.Top = fuelDispenser.Bottom;

                                if (!activeCar.IsBypassingObject)
                                {
                                    activeCar.IsBypassingObject = true;
                                    // choose where to bypass 
                                    newDestX = destPoint.X < activeCar.Left
                                        ? fuelDispenser.Left - (activeCar.Width + 5)
                                        : fuelDispenser.Right + (activeCar.Width + 5);


                                    if (destPoint.X < activeCar.Left)
                                    {
                                        newDestX = fuelDispenser.Left - (activeCar.Width + 5);
                                        bypassFromLeft = true;
                                    }
                                    else
                                    {
                                        newDestX = fuelDispenser.Right + (activeCar.Width + 5);
                                        bypassFromRight = true;
                                    }

                                    newDestY = fuelDispenser.Bottom + 10;


                                    newDestinationPoint1 = new Point(newDestX,
                                        newDestY);

                                    newDestY = fuelDispenser.Top + activeCar.Height + 10;
                                    newDestinationPoint2 = new Point(newDestX,
                                        newDestY);

                                    activeCar.DeleteDestinationSpot(_modelingForm);
                                    activeCar.AddDestinationPoint(newDestinationPoint2);
                                    activeCar.AddDestinationPoint(newDestinationPoint1);
                                }

                                break;
                            }

                        case DirectionEnum.Direction.Right:
                            {
                                break;
                            }

                        case DirectionEnum.Direction.Down:
                            {
                                activeCar.Top = fuelDispenser.Top - activeCar.Height;

                                if (!activeCar.IsBypassingObject)
                                {
                                    activeCar.IsBypassingObject = true;


                                    newDestX = fuelDispenser.Left - (activeCar.Width + 5);

                                    newDestY = fuelDispenser.Top - 10;

                                    newDestinationPoint1 = new Point(newDestX,
                                        newDestY);

                                    activeCar.DeleteDestinationSpot(_modelingForm);
                                    activeCar.AddDestinationPoint(newDestinationPoint1);
                                }

                                break;
                            }

                        case DirectionEnum.Direction.Left:
                            {
                                activeCar.Left = fuelDispenser.Right;

                                if (!activeCar.IsBypassingObject)
                                {
                                    activeCar.IsBypassingObject = true;


                                    newDestX = fuelDispenser.Right + 10;
                                    newDestY = fuelDispenser.Top - (ElementSizeDefiner.CarHeight + 5);

                                    newDestinationPoint1 = new Point(newDestX,
                                        newDestY);

                                    if (activeCar.IsFilled)
                                    {
                                        newDestX = fuelDispenser.Left - ElementSizeDefiner.CarWidth - 5;
                                    }
                                    else
                                    {
                                        activeCar.IsGoesHorizontal = true;
                                        //newDestX = fuelDispenser.Left - CarWidth - 5 - TopologyCellSize;
                                        newDestX = initialDestinationPoint.X + ElementSizeDefiner.TopologyCellSize / 2;
                                    }

                                    newDestinationPoint2 = new Point(newDestX,
                                        newDestY);

                                    activeCar.FromLeftBypassingPoint = newDestinationPoint2;

                                    newDestinationPoint3 = new Point(fuelDispenser.Left - 20,
                                        destPoint.Y - 20);

                                    activeCar.DeleteDestinationSpot(_modelingForm);
                                    //activeCarView.AddDestinationPoint(newDestinationPoint3);
                                    activeCar.AddDestinationPoint(newDestinationPoint2);
                                    activeCar.AddDestinationPoint(newDestinationPoint1);
                                }

                                break;
                            }
                    }
                }
            }

            return activeCar.GetDestinationPoint();
        }
    }
}
