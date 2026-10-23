using VectorEditor.Models;
using VectorEditor.Patterns;

namespace VectorEditor;

/// <summary>Сценарии демо-стенда: по одному блоку на каждый паттерн и каждое требование задания.</summary>
public static class Demo
{
    public static void Run()
    {
        var scene = BuildScene();
        Section("Сценарий 1. Composite: сцена из фигур и вложенных групп");
        ShowScene(scene);

        ScenarioMoveAndScale(scene);
        ScenarioExport(scene);
        ScenarioNewOperation(scene);
        ScenarioValidation(scene);
    }

    // --- сценарий 1: построение сцены ---
    private static ShapeGroup BuildScene()
    {
        var house = new ShapeGroup("Дом")
            .Add(new Rectangle("Стены", 100, 100, 80, 60))
            .Add(new Line("Крыша-лево", 100, 100, 140, 70))
            .Add(new Line("Крыша-право", 140, 70, 180, 100));

        var sun = new ShapeGroup("Солнце")
            .Add(new Circle("Диск", 220, 40, 15))
            .Add(new Line("Луч", 220, 15, 220, 5));

        var scene = new ShapeGroup("Сцена")
            .Add(house)
            .Add(sun)
            .Add(new Circle("Облако", 40, 40, 20));

        Console.WriteLine("  Собрана сцена: дом (прямоугольник + 2 линии), солнце (круг + линия) и отдельное облако.");
        Console.WriteLine("  Группы вложены в группу «Сцена», фигуры и группы используются через один интерфейс IShape.");
        return scene;
    }

    // --- сценарий 2: перемещение и масштабирование группы ---
    private static void ScenarioMoveAndScale(ShapeGroup scene)
    {
        Section("Сценарий 2. Группа ведёт себя как одна фигура");

        var house = (ShapeGroup)scene.Children[0];

        Console.WriteLine("  Команда: Move(+50, +20) для группы «Дом»");
        house.Move(50, 20);
        ShowScene(house);

        var bounds = house.Accept(new BoundsVisitor())!.Value;
        Console.WriteLine($"\n  Команда: Scale(×2) для группы «Дом» относительно её центра ({bounds.CenterX:0.##}; {bounds.CenterY:0.##})");
        house.Scale(2, bounds.CenterX, bounds.CenterY);
        ShowScene(house);

        Console.WriteLine("\n  Команда: Move(-10, 0) для всей сцены (вложенные группы двигаются вместе)");
        scene.Move(-10, 0);
        ShowScene(scene);
    }

    // --- сценарий 3: экспорт в два формата ---
    private static void ScenarioExport(ShapeGroup scene)
    {
        Section("Сценарий 3. Visitor: экспорт сцены в два формата");

        Console.WriteLine("  Формат 1: SVG-подобный текст (SvgExportVisitor)\n");
        Console.WriteLine(new SvgExportVisitor().Export(scene, 400, 300));

        Console.WriteLine("\n  Формат 2: JSON (JsonExportVisitor)\n");
        Console.WriteLine(new JsonExportVisitor().Export(scene));
    }

    // --- сценарий 4: новая операция без правки фигур ---
    private static void ScenarioNewOperation(ShapeGroup scene)
    {
        Section("Сценарий 4. Новая операция без изменения классов фигур");

        Console.WriteLine("  BoundsVisitor и OutlineVisitor — отдельные классы; Circle/Rectangle/Line/ShapeGroup не менялись.");
        var b = scene.Accept(new BoundsVisitor());
        if (b is { } bounds)
            Console.WriteLine($"  Габариты сцены: x {bounds.MinX:0.##}…{bounds.MaxX:0.##}, y {bounds.MinY:0.##}…{bounds.MaxY:0.##} " +
                              $"({bounds.Width:0.##}×{bounds.Height:0.##})");

        Console.WriteLine("  Дерево сцены (OutlineVisitor):");
        ShowScene(scene);
    }

    // --- сценарий 5: защита от неверных операций ---
    private static void ScenarioValidation(ShapeGroup scene)
    {
        Section("Сценарий 5. Проверка некорректных действий");

        Try("Круг с радиусом 0", () => new Circle("Плохой", 0, 0, 0));
        Try("Масштаб ×0 для группы", () => scene.Scale(0, 0, 0));
        Try("Добавить группу саму в себя", () => scene.Add(scene));
        Try("Добавить «Сцену» внутрь вложенной группы «Дом»", () => ((ShapeGroup)scene.Children[0]).Add(scene));
        Try("Добавить фигуру повторно", () => scene.Add(scene.Children[2]));
    }

    private static void Try(string title, Action action)
    {
        try
        {
            action();
            Console.WriteLine($"  {title}: выполнено (неожиданно)");
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            Console.WriteLine($"  {title}: отклонено — {ex.Message.Split('\n')[0]}");
        }
    }

    private static void ShowScene(IShape shape)
    {
        var text = shape.Accept(new OutlineVisitor());
        foreach (var line in text.Split('\n'))
            Console.WriteLine("  " + line);
    }

    private static void Section(string title)
    {
        Console.WriteLine();
        Console.WriteLine(new string('=', 72));
        Console.WriteLine(title);
        Console.WriteLine(new string('=', 72));
    }
}
