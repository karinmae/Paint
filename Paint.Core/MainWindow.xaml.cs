﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Paint.Core.Models;
using Paint.Core.ViewModels;
using Paint.Core.Command;
using Paint.Core.Helper;
using Paint.Core.Composite;


namespace Paint.Core
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Variables
        MatrixTransform transform;
        //RectangleShape rectangle;
        //EllipseShape ellipse;
        Path tempForm;
        Brush brush;


        HitTestResult resize;
        

        ShapeComposite composite;
        Options option = new Options();

        int newShapeIndex;

        Point StartMovePoint;
        Point EndMovePoint;

        GenericShape select = new GenericShape();

        private MVVMViewModel dataContainer = new MVVMViewModel();
        CanvasShapeContainer shapeContainer;

        #endregion

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = dataContainer;

            shapeContainer = new CanvasShapeContainer();
            transform = new MatrixTransform();
            tempForm = new Path();
            newShapeIndex = 0;

            composite = new ShapeComposite();
        }

        #region define the mode

        private void MenuItem_ResizeMode(object sender, RoutedEventArgs args)
        {

        }

        private void MenuItem_DragAndDropMode(object sender, RoutedEventArgs args)
        {
            option.type = Options.op.moveShape;
        }
        private void MenuItem_Regtangle(object sender, RoutedEventArgs e)
        {
            option.type = Options.op.newShape_regtangle;
        }

        private void MenuItem_Ellipse(object sender, RoutedEventArgs e)
        {
            option.type = Options.op.newShape_ellipse;
        }

        private void MenuItem_Pencil(object sender, RoutedEventArgs e)
        {
            option.type = Options.op.newLine_Pencil;
        }

        private void MenuItem_Marker(object sender, RoutedEventArgs e)
        {
            option.type = Options.op.newLine_Marker;
        }

        private void MenuItem_Group(object sender, RoutedEventArgs e)
        {
            option.type = Options.op.groupShape;
        }

        #endregion

        private void Scene_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ICommands com;
            HitTestResult result = VisualTreeHelper.HitTest(Scene, Mouse.GetPosition(Scene));
            resize = result;
            Path path = result.VisualHit as Path;

            StartMovePoint = e.GetPosition(Scene);
            double posX = e.GetPosition(Scene).X;
            double posY = e.GetPosition(Scene).Y;


            switch (option.type)
            {
                case Options.op.newShape_regtangle:
                    com = new CommandCreateRect(posX, posY, newShapeIndex, shapeContainer, brush);
                    com.Execute();
                    break;

                case Options.op.newShape_ellipse:
                    com = new CommandCreateEllipse(posX, posY, newShapeIndex, shapeContainer, brush);
                    com.Execute();
                    break;

                case Options.op.newLine_Pencil:
                    com = new CommandCreatePencil(StartMovePoint, StartMovePoint, newShapeIndex, shapeContainer, brush);
                    com.Execute();
                    break;

                case Options.op.newLine_Marker:
                    com = new CommandCreateMarker(StartMovePoint, StartMovePoint, newShapeIndex, shapeContainer, brush);
                    com.Execute();
                    break;

                case Options.op.moveShape:
                    if (path != null)
                    {
                        path.Fill = Brushes.CadetBlue;
                    }
                    break;

                case Options.op.groupShape:
                    
                    com = new CommandSelectStart(posX, posY, select, Brushes.Transparent);
                    com.Execute();
                    break;

                case Options.op.resize:

                    break;
            }
        }

        private void Scene_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ICommands com;

            EndMovePoint = e.GetPosition(Scene);
            double dX = EndMovePoint.X - StartMovePoint.X;
            double dY = EndMovePoint.Y - StartMovePoint.Y;

            switch (option.type)
            {
                case Options.op.newShape_regtangle:
                    if (shapeContainer.shapes.ContainsKey(newShapeIndex))
                    {
                        com = new CommandFinalizeRect(EndMovePoint.X, EndMovePoint.Y, newShapeIndex, shapeContainer, brush);
                        com.Execute();
                        newShapeIndex++;
                        Render();
                    }
                    break;

                case Options.op.newShape_ellipse:
                    if (shapeContainer.shapes.ContainsKey(newShapeIndex))
                    {
                        com = new CommandFinalizeEllipse(EndMovePoint.X, EndMovePoint.Y, newShapeIndex, shapeContainer, brush);
                        com.Execute();
                        newShapeIndex++;
                        Render();
                    }
                    break;


                case Options.op.newLine_Pencil:
                    if (shapeContainer.shapes.ContainsKey(newShapeIndex))
                    {
                        com = new CommandFinalizePencil(StartMovePoint, EndMovePoint, newShapeIndex, shapeContainer, brush);
                        com.Execute();
                        newShapeIndex++;
                        Render();
                    }
                    break;

                case Options.op.newLine_Marker:
                    if (shapeContainer.shapes.ContainsKey(newShapeIndex))
                    {
                        com = new CommandFinalizeMarker(StartMovePoint, EndMovePoint, newShapeIndex, shapeContainer, brush);
                        com.Execute();
                        newShapeIndex++;
                        Render();
                    }
                    break;

                case Options.op.resize:
                    Path path_r = resize.VisualHit as Path;
                    int index_r = Convert.ToInt32(path_r.Tag.ToString());

                    if (path_r.Data.GetType() == typeof(RectangleGeometry))
                    {
                        int index = Convert.ToInt32(path_r.Tag.ToString());
                        GenericShape form = shapeContainer.shapes[index];

                        if (composite.child.Contains(form))
                        {
                            foreach (var pair in composite.child)
                            {
                                int indexGroup = Convert.ToInt32(pair.Shape.Tag);
                                com = new CommandResizeRect(dX, dY, indexGroup, shapeContainer, pair.Shape, Brushes.Blue);
                                com.Execute();
                            }
                        }
                        else
                        {
                            com = new CommandResizeRect(dX, dY, index_r, shapeContainer, path_r, Brushes.Blue);
                            com.Execute();
                        }
                        Render();
                    }

                    break;

                case Options.op.moveShape:

                    HitTestResult result = VisualTreeHelper.HitTest(Scene, Mouse.GetPosition(Scene));
                    Path path = result.VisualHit as Path;

                    if (path.Data.GetType() == typeof(RectangleGeometry))
                    {
                        int index = Convert.ToInt32(path.Tag.ToString());
                        GenericShape form = shapeContainer.shapes[index];

                        if (composite.child.Contains(form))
                        {
                            foreach (var pair in composite.child)
                            {
                                int indexGroup = Convert.ToInt32(pair.Shape.Tag);

                                com = new CommandMoveRect(dX, dY, indexGroup, shapeContainer, pair.Shape, brush);
                                com.Execute();
                            }
                        }
                        else{
                            com = new CommandMoveRect(dX, dY, index, shapeContainer, path, brush);
                            com.Execute();
                        }
                    }

                 else if (path.Data.GetType() == typeof(EllipseGeometry))
                    {
                        int index = Convert.ToInt32(path.Tag.ToString());
                        GenericShape form = shapeContainer.shapes[index];

                        if (composite.child.Contains(form))
                        {
                            foreach (var pair in composite.child)
                            {
                                int indexGroup = Convert.ToInt32(pair.Shape.Tag);
                                com = new CommandMoveEllipse(dX, dY, indexGroup, shapeContainer, pair.Shape, brush);
                                com.Execute();
                            }
                        }
                        else{
                            com = new CommandMoveEllipse(dX, dY, index, shapeContainer, path, brush);
                            com.Execute();
                        }
                        Render();
                    }
                    break;

                case Options.op.groupShape:
                    ShapeComposite group = new ShapeComposite();
                   
                    foreach (var pair in shapeContainer.shapes)
                    {
                        GenericShape temp = new GenericShape();
                        temp = pair.Value;
                        if ((temp.position.X > StartMovePoint.X) && (temp.position.Y > StartMovePoint.Y) && (temp.position.X < EndMovePoint.X) && (temp.position.Y < EndMovePoint.Y))
                        {
                            //group.add(temp);
                            composite.add(temp);
                            //Scene.Children.Add(temp.Shape);
                        }
                    }

                    //composite.add(group);
                    Scene.Children.Remove(select.Shape);
                    Render();
                    break;
            }
        }

        private void Scene_MouseMove(object sender, MouseEventArgs e)
        {
            double posX = e.GetPosition(Scene).X;
            double posY = e.GetPosition(Scene).Y;

            switch (option.type)
            {
                case Options.op.newShape_regtangle:
                    if (Mouse.LeftButton == MouseButtonState.Pressed)
                    {
                        if (shapeContainer.shapes.ContainsKey(newShapeIndex))
                        {
                            if (Scene.Children.Contains(tempForm))
                                Scene.Children.Remove(tempForm);

                            CommandSetSizeRect com = new CommandSetSizeRect(posX, posY, newShapeIndex, shapeContainer, brush);
                            com.Execute();
                            tempForm = com.getShape();
                            Scene.Children.Add(tempForm);
                        }
                    }
                    break;

                case Options.op.newShape_ellipse:
                    if (Mouse.LeftButton == MouseButtonState.Pressed)
                    {
                        if (shapeContainer.shapes.ContainsKey(newShapeIndex))
                        {
                            if (Scene.Children.Contains(tempForm))
                                Scene.Children.Remove(tempForm);

                            CommandSetSizeEllipse com = new CommandSetSizeEllipse(posX, posY, newShapeIndex, shapeContainer, brush);
                            com.Execute();
                            tempForm = com.getShape();
                            Scene.Children.Add(tempForm);
                        }
                    }
                    break;

                case Options.op.moveShape:
                    

                    if (Mouse.LeftButton == MouseButtonState.Pressed)
                    {
                        HitTestResult result = VisualTreeHelper.HitTest(Scene, Mouse.GetPosition(Scene));
                        Path path = result.VisualHit as Path;

                        int index = Convert.ToInt32(path.Tag.ToString());
                        GenericShape form = shapeContainer.shapes[index];

                        if (composite.child.Contains(form))                     
                        {
                            foreach (var pair in composite.child)
                            {
                                transform.Matrix = new Matrix(1, 0, 0, 1,
                                      posX - StartMovePoint.X,
                                      posY - StartMovePoint.Y);
                                pair.Shape.RenderTransform = transform;
                            }
                        }
                        else if(path != null)
                        {
                            transform.Matrix = new Matrix(1, 0, 0, 1,
                                 posX - StartMovePoint.X,
                                 posY - StartMovePoint.Y);
                            path.RenderTransform = transform;
                        }
                    }
                    break;

                case Options.op.groupShape:
                    if (Mouse.LeftButton == MouseButtonState.Pressed)
                    {
                            if (Scene.Children.Contains(select.Shape))
                                Scene.Children.Remove(select.Shape);

                            CommandSelectSize com = new CommandSelectSize(posX, posY, select, Brushes.Transparent);
                            com.Execute();
                            select.Shape = com.getShape();
                            Scene.Children.Add(select.Shape);
                        
                    }
                    break;
            }
        }

        private void Render()
        {
            Scene.Children.Clear();
            foreach (var pair in shapeContainer.shapes)
            {
                GenericShape temp = pair.Value;
                Scene.Children.Add(temp.Shape);
            }
        }

        private void AddRectangle_Click(object sender, RoutedEventArgs e)
        {
            option.type = Options.op.newShape_regtangle;
        }

        private void AddCircle_Click(object sender, RoutedEventArgs e)
        {
            option.type = Options.op.newShape_ellipse;
        }

        private void Pencil_Click(object sender, RoutedEventArgs e)
        {
            option.type = Options.op.newLine_Pencil;
        }

        private void Marker_Click(object sender, RoutedEventArgs e)
        {
            option.type = Options.op.newLine_Marker;
        }

        private void Group_Click(object sender, RoutedEventArgs e)
        {
            option.type = Options.op.groupShape;
        }

        private void Move_Click(object sender, RoutedEventArgs e)
        {
            option.type = Options.op.moveShape;
        }


        private void Resize_Click(object sender, RoutedEventArgs e)
        {
            option.type = Options.op.resize;
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
           
            var button = sender as RadioButton;
            string color = button.Content.ToString();
            var converter = new System.Windows.Media.BrushConverter();
            brush = (Brush)converter.ConvertFromString(color);
        }


       
    }

}
