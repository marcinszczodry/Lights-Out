Imports System.Drawing
Module Module1

    ' GRID SETTINGS '
    Dim gridSize As Integer = 5 'Grid model (e.g. 5 => 5x5) and
    Dim gridCell As Integer = 4 'Cell size (e.g. 2 => 4x2 to visually represent the cell being a square )
    ' END OF GRID SETTINGS ' 

    Dim gridPosX, gridPosY As Integer ' Grid start point based on window size
    Dim gridWidth, gridHeight As Integer ' Grid's width and height (total, including margins)

    Dim lights(1, 1) As Boolean  ' Store lights as cells
    Dim cursor(1) As Integer

    Dim timer As DateTime
    Dim steps As Integer = 0

    ' Set window size
    Sub SetWindowSize(width, height)
        Console.SetWindowSize(width, height)
    End Sub

    ' Draw grid
    Sub DrawGrid(size, cell)

        Dim height As Integer = cell
        Dim width As Integer = cell * 2

        gridCell = cell
        gridSize = size

        gridWidth = size * width + size + 1 ' calculate grid width based on the number of rows and the width of each
        gridHeight = size * height + size + 1 ' calculate grid height based on the number of cols and height of each

        gridPosX = Math.Floor((Console.WindowWidth - gridWidth) / 2) ' calculate horizontal start point, where grid will be drawn from
        gridPosY = Console.CursorTop + 1 ' calculate vertical start point, where grid will be drawn from

        ' Horizontal lines
        For v = 0 To gridHeight - 1 Step height + 1
            For h = 0 To gridWidth - 1
                Console.SetCursorPosition(gridPosX + h, gridPosY + v)
                Console.Write("*")
            Next
        Next

        ' Vertical lines
        For h = 0 To gridWidth - 1 Step width + 1
            For v = 0 To gridHeight - 1
                Console.SetCursorPosition(gridPosX + h, gridPosY + v)
                Console.Write("*")
            Next
        Next

    End Sub

    ' Listen for user movement (left, right, up, down, enter,escape)
    Function ListenForMovement()
        Dim move, key As ConsoleKey
        Dim isProperKey As Boolean = False

        ' Repeat untill proper keys are pressed
        Do
            key = Console.ReadKey(True).Key
            Select Case key
                Case ConsoleKey.UpArrow
                    move = ConsoleKey.UpArrow
                    isProperKey = True
                Case ConsoleKey.DownArrow
                    move = ConsoleKey.DownArrow
                    isProperKey = True
                Case ConsoleKey.LeftArrow
                    move = ConsoleKey.LeftArrow
                    isProperKey = True
                Case ConsoleKey.RightArrow
                    move = ConsoleKey.RightArrow
                    isProperKey = True
                Case ConsoleKey.Enter
                    move = ConsoleKey.Enter
                    isProperKey = True
                Case ConsoleKey.Escape
                    move = ConsoleKey.Escape
                    isProperKey = True
                Case Else
                    isProperKey = False
            End Select
        Loop While isProperKey = False

        Return move
    End Function

    ' Draw pointer
    Sub DrawPointer()

        ' Save cursor position, so it cna be loaded after the pointer is drawn
        SaveCursorPos()

        ' Calculate the position in the array, based on the physical values from 'left' and 'right'
        Dim position(1) As Integer
        position = getFriendlyPosition(Console.CursorLeft, Console.CursorTop)

        ' Change the background of pointer to yellow, if the cell is light-one. Also change the colour to darker
        If IsLight(position(0), position(1)) Then
            Console.BackgroundColor = ConsoleColor.Yellow
            Console.ForegroundColor = ConsoleColor.Black
        End If

        ' Draw pointer look
        For cell = 0 To gridCell - 1
            LoadCursorPos()
            Console.SetCursorPosition(Console.CursorLeft, Console.CursorTop + cell)
            If (cell = 0) Then
                Console.Write("\")
                Console.Write(New String(" ", (gridCell * 2) - 2))
                Console.Write("/")
            ElseIf (cell = gridCell - 1) Then
                Console.Write("/")
                Console.Write(New String(" ", (gridCell * 2) - 2))
                Console.Write("\")
            Else
                Console.WriteLine(New String(" ", (gridCell * 2)))
            End If

        Next

        ' Load cursor position, to avoid problems with destroyed program
        LoadCursorPos()

    End Sub

    ' Clear pointer
    Sub ClearPointer()

        ' Save cursor position, so it can be loaded after the pointer is cleared
        SaveCursorPos()

        ' Clear pointer look
        For cell = 0 To gridCell - 1
            LoadCursorPos()
            Console.SetCursorPosition(Console.CursorLeft, Console.CursorTop + cell)
            Console.WriteLine(New String(" ", (gridCell * 2)))
        Next

        ' Set cursor position to top left corner of cell, to avoid layout destruction
        LoadCursorPos()

        Console.ResetColor()

    End Sub

    ' Clear previous selection, fill new cell, ONLY if user's move is within the grid
    Sub SelectCell(direction)
        ' User moves down
        If (direction = ConsoleKey.DownArrow) And (Console.CursorTop < (gridHeight - gridCell - 1 + gridPosY)) Then
            ClearPointer()
            Console.SetCursorPosition(Console.CursorLeft, Console.CursorTop + gridCell + 1)
            DrawPointer()
        End If
        'User moves up
        If (direction = ConsoleKey.UpArrow) And (Console.CursorTop > (gridPosY + gridCell)) Then
            ClearPointer()
            Console.SetCursorPosition(Console.CursorLeft, Console.CursorTop - gridCell - 1)
            DrawPointer()
        End If
        ' User moves left
        If (direction = ConsoleKey.LeftArrow) And (Console.CursorLeft > (gridPosX + gridCell * 2 - 1)) Then
            ClearPointer()
            Console.SetCursorPosition(Console.CursorLeft - gridCell * 2 - 1, Console.CursorTop)
            DrawPointer()
        End If
        ' User moves right
        If (direction = ConsoleKey.RightArrow) And (Console.CursorLeft < (gridPosX + gridWidth - gridCell * 2 - 1)) Then
            ClearPointer()
            Console.SetCursorPosition(Console.CursorLeft + gridCell * 2 + 1, Console.CursorTop)
            DrawPointer()
        End If
    End Sub

    ' Generate random light positions
    Sub GenerateRandomLights()
        Dim x As Integer ' position of the cell from left side of the grid
        Dim y As Integer ' position of the cell from top side of the grid

        ' Generate random position between 1 and the half of total number of cells, based on the grid size
        Randomize()
        For r = 0 To Math.Floor((lights.Length - 1) / 2)
            x = ((gridSize - 1) * Rnd())
            y = ((gridSize - 1) * Rnd())
            lights(x, y) = True ' Assign light to cell
        Next
    End Sub

    ' Draw light
    Sub DrawLight(position)
        Console.BackgroundColor = ConsoleColor.Yellow

        ' Fill with yellow colour by putting spaces
        For cell = 0 To gridCell - 1
            Console.SetCursorPosition(position(0), position(1) + cell)
            Console.WriteLine(New String(" ", (gridCell * 2)))
        Next
        Console.ResetColor()

    End Sub

    ' Clear light
    Sub ClearLight(position)
        Console.ResetColor()

        ' Fill with yellow colour by putting spaces
        For cell = 0 To gridCell - 1
            Console.SetCursorPosition(position(0), position(1) + cell)
            Console.WriteLine(New String(" ", (gridCell * 2)))
        Next

    End Sub

    ' Update lights
    Sub UpdateLights()
        Dim position(1) As Integer
        SaveCursorPos()

        For x = 0 To lights.GetLength(0) - 1
            For y = 0 To lights.GetLength(1) - 1
                position = getPosition(x, y)
                If IsLight(x, y) Then
                    DrawLight(position)
                Else
                    ClearLight(position)
                End If
            Next
        Next
        LoadCursorPos()
    End Sub

    ' Count lights that are turned on
    Function CountLights()

        Dim numberOfLights As Integer = 0

        For x = 0 To gridSize - 1
            For y = 0 To gridSize - 1
                Debug.WriteLine("x: " & x & " ~ y: " & y & " ~ is light: " & IsLight(x, y))
                If IsLight(x, y) Then

                    numberOfLights += 1
                End If
            Next
        Next
        Return numberOfLights
    End Function

    ' Generate position based on x and y (from grid size)
    Function getPosition(x, y)
        Dim position(1) As Integer
        Dim xPos As Integer = gridPosX + 1 + x * (gridCell * 2 + 1)
        Dim yPos As Integer = gridPosY + 1 + y * (gridCell + 1)
        position = {xPos, yPos}
        Return position
    End Function

    ' Generate light position in the array based on physcial cursor position
    Function getFriendlyPosition(xPos, yPos)
        Dim x As Integer = ((gridWidth - gridSize * gridCell * 2 - gridSize - 1) + (xPos - gridPosX - 1)) / (gridCell * 2 + 1)
        Dim y As Integer = ((gridHeight - gridSize * gridCell - gridSize - 1) + (yPos - gridPosY - 1)) / (gridCell + 1)
        Return {x, y}
    End Function

    ' Check if cell is the 'light one'
    Function IsLight(x, y)
        Dim status As Boolean = False
        If lights(x, y) = True Then
            status = True
        Else
            status = False
        End If
        Return status
    End Function

    ' Save cursor position
    Sub SaveCursorPos()
        cursor(0) = Console.CursorLeft
        cursor(1) = Console.CursorTop
    End Sub

    ' Load cursor position
    Sub LoadCursorPos()
        Console.CursorTop = cursor(1)
        Console.CursorLeft = cursor(0)
    End Sub

    ' Switch light of selected cell and neigbouring cells
    Sub switchLights()

        Dim pos(1) As Integer
        Dim x, y As Integer
        x = getFriendlyPosition(cursor(0), cursor(1))(0)
        y = getFriendlyPosition(cursor(0), cursor(1))(1)

        'Save cursor pos so it can be accessed later
        SaveCursorPos()

        ' Switch the light of current cell
        lights(x, y) = Not IsLight(x, y)

        ' Switch light of neighbours
        If (x > 0) And x < gridSize - 1 Then
            lights(x - 1, y) = Not IsLight(x - 1, y)
            lights(x + 1, y) = Not IsLight(x + 1, y)
        End If

        If (y > 0) And y < gridSize - 1 Then
            lights(x, y - 1) = Not IsLight(x, y - 1)
            lights(x, y + 1) = Not IsLight(x, y + 1)
        End If

        If (x = 0) Then
            lights(x + 1, y) = Not IsLight(x + 1, y)
        End If

        If (x = gridSize - 1) Then
            lights(x - 1, y) = Not IsLight(x - 1, y)
        End If

        If (y = 0) Then
            lights(x, y + 1) = Not IsLight(x, y + 1)
        End If

        If (y = gridSize - 1) Then
            lights(x, y - 1) = Not IsLight(x, y - 1)
        End If



    End Sub

    ' Labels
    Sub ProcessLabels()
        Dim str As String
        str = "Steps made: " + steps.ToString

        Console.SetCursorPosition((Console.WindowWidth - str.Length - 1) / 2, 2)
        Console.Write(New String(str))
        Console.WriteLine()
    End Sub

    ' Write tetx centered
    Sub WriteCentered(str)
        Console.SetCursorPosition((Console.WindowWidth - str.length) / 2, Console.CursorTop)
        Console.WriteLine(str)
    End Sub

    ' Margin top
    Sub MarginTop(val)
        Console.SetCursorPosition(0, Console.CursorTop + val)
    End Sub

    ' Splashscreen
    Sub Splashscreen()
        Dim name As String = "L I G H T S   O U T"

        Console.SetCursorPosition(0, (Console.WindowHeight - 1) / 2)

        Console.ForegroundColor = ConsoleColor.Black
        WriteCentered(name)
        Threading.Thread.Sleep(700)
        Console.Clear()

        Console.SetCursorPosition(0, (Console.WindowHeight - 1) / 2)
        Console.ForegroundColor = ConsoleColor.DarkYellow
        WriteCentered(name)
        Threading.Thread.Sleep(600)
        Console.Clear()

        Console.SetCursorPosition(0, (Console.WindowHeight - 1) / 2)
        Console.ForegroundColor = ConsoleColor.Yellow
        WriteCentered(name)
        Threading.Thread.Sleep(1500)
        Console.Clear()

        Console.ResetColor()
        Console.ForegroundColor = ConsoleColor.Gray

    End Sub

    ' Game screen
    Sub Game()

        Dim move As ConsoleKey ' Store user key input
        Dim isWon As Boolean = False ' Store the status of the game
        Dim isExit As Boolean = False
        Dim isTimerRunning As Boolean = False

        ' Draw labels
        steps = 0
        ProcessLabels()

        ' Draw grid on the screen (5 rows x 5 columns, whereas each cell visually represents 2x2 size)
        DrawGrid(gridSize, gridCell)

        ' Label exit
        MarginTop(2)
        Console.BackgroundColor = ConsoleColor.White
        Console.ForegroundColor = ConsoleColor.Black
        WriteCentered("Press ESC to go back to main menu")
        Console.ResetColor()

        'By default select first cell of the grid
        Console.SetCursorPosition(gridPosX + 1, gridPosY + 1)

        ' Update the size of the array, so it matches the number of cells of the grid. (e.g. 5x5 grid will resize the array to be 25 long: 0-24)
        ReDim lights(gridSize - 1, gridSize - 1)

        ' Generate random lights and display them
        GenerateRandomLights()
        UpdateLights()

        ' Go back to first cell
        Console.SetCursorPosition(gridPosX + 1, gridPosY + 1)

        ' Until user wants to exit
        Do

            DrawPointer()
            ' Listen and return user movement, ONLY if approprate key is pressed
            move = ListenForMovement()

            ' Save the time when user makes the first step
            If Not isTimerRunning Then
                timer = DateTime.Now
                isTimerRunning = True
            End If

            ' Exit program on escape
            If move = ConsoleKey.Escape Then
                Console.ResetColor()
                Statistics()
            End If

            ' Decide whether to switch the lights or move pointer in appropriate direction
            If move = ConsoleKey.Enter Then

                'Switch light of current cell And neighbouring cells 
                switchLights()
                UpdateLights()

                ' Start counting steps made and update them evey time user presses enter
                steps = steps + 1
                ProcessLabels()

                ' Load cursors pos to avoid layout destruction
                LoadCursorPos()

            Else
                ' Select cell
                SelectCell(move)
            End If

            ' Check if won
            For Each cell In lights
                If cell = True Then
                    isWon = False
                    Exit For
                Else
                    isWon = True
                End If
            Next

            ' If user won, get the new time and calculate the difference
            If isWon Then
                Statistics(True)
            End If

        Loop Until isExit
    End Sub

    ' Statistics
    Sub Statistics(Optional isWin As Boolean = False)

        Dim countedLights = CountLights()

        Console.Clear()
        Console.SetCursorPosition(gridPosX, gridPosY - 1)
        DrawGrid(gridSize, gridCell)

        If Not isWin Then
            'clear lights
            For i = 0 To gridSize - 1
                For j = 0 To gridSize - 1
                    lights(i, j) = False
                    UpdateLights()
                    Threading.Thread.Sleep(30)
                Next

            Next
        End If

        Threading.Thread.Sleep(200)
        Console.SetCursorPosition(gridPosX, gridPosY - 1)
        Console.ForegroundColor = ConsoleColor.DarkGray
        DrawGrid(gridSize, gridCell)
        Threading.Thread.Sleep(200)
        Console.SetCursorPosition(gridPosX, gridPosY - 1)
        Console.ForegroundColor = ConsoleColor.Black
        DrawGrid(gridSize, gridCell)
        Threading.Thread.Sleep(200)
        Console.Clear()

        Dim newTime As DateTime = DateTime.Now
        Dim difference As TimeSpan = newTime - timer
        Dim timetaken As String = "Time taken: " & difference.ToString("mm") & "m, " & difference.ToString("ss") & "s"
        Dim stepsmade As String = "Steps made: " & steps
        Dim lightsleft As String = "Lights left: " & countedLights
        Dim str As String

        Console.ForegroundColor = ConsoleColor.White
        Console.SetCursorPosition(0, (Console.WindowHeight - 3) / 2)
        If isWin Then
            Console.ForegroundColor = ConsoleColor.Green
            str = "You won! :)"
        Else
            Console.ForegroundColor = ConsoleColor.Red
            str = "You lost! :("
        End If

        WriteCentered(str)
        Console.ResetColor()
        WriteCentered(timetaken)
        WriteCentered(stepsmade)
        WriteCentered(lightsleft)

        Console.SetCursorPosition(0, Console.CursorTop + 2)
        Console.BackgroundColor = ConsoleColor.White
        Console.ForegroundColor = ConsoleColor.Black
        WriteCentered(" Press RETURN to go back to main menu. ")

        Dim isKeyPressed As Boolean = False
        Dim storeKey As ConsoleKey
        Do
            storeKey = Console.ReadKey(True).Key
            If storeKey = ConsoleKey.Enter Then
                isKeyPressed = True
            Else
                isKeyPressed = False
            End If

        Loop Until isKeyPressed

        Console.ResetColor()
        Console.Clear()
        Menu()
    End Sub

    ' Draw menu
    Function DrawMenu(links, selection)

        Dim startPos, endPos As Integer

        ' Clear menu
        Console.Clear()

        ' Heading
        MarginTop((Console.WindowHeight - 6) / 2)
        WriteCentered("MENU")
        MarginTop(2)

        ' Draw links
        For y = 0 To links.Length - 1

            If y = 0 Then
                startPos = Console.CursorTop
            End If

            If y = links.length - 1 Then
                endPos = Console.CursorTop
            End If

            If y = selection Then
                SaveCursorPos()
                Console.BackgroundColor = ConsoleColor.Yellow
                Console.ForegroundColor = ConsoleColor.Black
            Else
                Console.BackgroundColor = ConsoleColor.Black
                Console.ForegroundColor = ConsoleColor.White
            End If
            WriteCentered(links(y))
        Next
        Console.ResetColor()
        Return {startPos, endPos}
    End Function

    ' Menu screen
    Sub Menu()
        Dim action As ConsoleKey
        Dim border() As Integer
        Dim selection As Integer = 0
        Dim links() As String
        links = {"PLAY", "HELP", "EXIT"}

        ' Until user press return key
        Do While (True)

            ' Draw menu, get start and end position
            border = DrawMenu(links, selection)

            ' Load active link
            LoadCursorPos()

            ' Listen for action (up / down / enter)
            action = Console.ReadKey(True).Key

            Select Case action
                Case ConsoleKey.DownArrow
                    If (Console.CursorTop < border(1)) Then
                        selection = selection + 1
                        Debug.WriteLine(selection)
                    End If
                Case ConsoleKey.UpArrow
                    If (Console.CursorTop > border(0)) Then
                        selection = selection - 1
                        Debug.WriteLine(selection)
                    End If
                Case ConsoleKey.Enter
                    Select Case selection
                        Case 0
                            Console.Clear()
                            Game()
                        Case 1
                            Console.Clear()
                            Help()
                        Case 2
                            End
                    End Select
            End Select

        Loop


    End Sub

    ' Help screen
    Sub Help()
        Console.CursorTop = 10
        Console.BackgroundColor = ConsoleColor.DarkGray
        Console.ForegroundColor = ConsoleColor.White
        WriteCentered(" H O W   T O   P L A Y ? ")
        Console.ResetColor()
        Console.CursorTop = 13
        WriteCentered("You are supposed to turn off all the lights on the " & gridSize & "x" & gridSize & " board.")
        WriteCentered("Turning off one light will turn on the lights of cells around it")
        WriteCentered("(with exception for diagonal cells).")
        WriteCentered("You can navigate on the board using the UP / DOWN / LEFT / RIGHT keys.")
        WriteCentered("You can switch the light of selected cell using the RETURN key.")
        Console.CursorTop = 19
        Console.BackgroundColor = ConsoleColor.White
        Console.ForegroundColor = ConsoleColor.Black
        WriteCentered("Press RETURN key to go back to main menu")
        Console.ResetColor()
        Console.ForegroundColor = ConsoleColor.Black
        Console.ReadLine()
        Console.ResetColor()
    End Sub

    ' Main procedure
    Sub Main()

        ' Hide cursor
        Console.CursorVisible = False

        ' Set window size
        SetWindowSize(80, 40)

        ' Display splashscreen
        Splashscreen()

        ' Display menu
        Menu()

    End Sub

End Module