Studentnaam: Fabian Hofman

Studentnummer: 598408

# Algemene beschrijving applicatie
Dit is een multichat applicatie voor de course NotS-WIN. Deze applicatie bestaat uit een server en meerdere clients.  Als client kan je verbinden met de server en kan je mee doen met het chatten. Zodra de server netjes wordt afgesloten, zal de server dit melden bij alle clients en ze allemaal disconnecten. Als een client disconnect van de server zal die dit ook netjes melden en zich afmelden bij de server. Bij het afsluiten van een van de 2 applicaties, zal dit goed opgevangen worden met exceptions en de juiste foutmeldingen gepresenteerd worden aan de gebruiker.

## Generics
Generics zijn toegevoegd sinds versie 2.0 van C# en de common language runtime (CLR). Generics zijn geïntroduceerd aan het .NET framework als parameter zijnde. Hiermee is het mogelijk om klassen en methodes te maken die verschillen in specificatie van een or meer type klassen of methodes die gemaakt worden door de code. Als voorbeeld, door het gebruiken van een generic type parameter T kan je een enkele klasse schrijven die andere code kan gebruiken zonder het risico van een runtime casts or moxing operation op te lopen, zoals hieronder te zien

```cs
// Declare the generic class.
public class GenericList<T>
{
    public void Add(T input) { }
}
class TestGenericList
{
    private class ExampleClass { }
    static void Main()
    {
        // Declare a list of type int.
        GenericList<int> list1 = new GenericList<int>();
        list1.Add(1);

        // Declare a list of type string.
        GenericList<string> list2 = new GenericList<string>();
        list2.Add("");

        // Declare a list of type ExampleClass.
        GenericList<ExampleClass> list3 = new GenericList<ExampleClass>();
        list3.Add(new ExampleClass());
    }
}
```


Generic klassen en methodes combineren het hergebruik, type safety en efficiëntie op een manier die een non-generic tegenhanger niet zou kunnen. Generics komen het meest voor bij het gebruik van collecties en methodes die gebruik van ze maken. Het wordt hierdoor ook aanbevolen om dit te gebruiken in plaats van zijn non-generic tegenhangers zoals List of ArrayList.

In een generic type of methode definitie, is een type parameter de placeholder voor een specifiek type die ingevuld wordt bij het maken van een instantie van het generic type. Een generic class, zoals `GenericList<T>` die in het bovestaande voorbeeld gebruik wordt, kan niet gebruik wordt als is omdat het niet echt een type is; het is meer een blueprint voor het type. Om gebruik te maken van `GenericList<T>`, moet dit in de code gedeclareerd zijn en geïnitialiseerd. Dit doe je door het type in het in brackets `<>` te plaatsen. Elk aantal geconstrueerde type-instanties kunnen als volgt worden gemaakt, elk met aan ander type argument:  

```cs
GenericList<float> list1 = new GenericList<float>();
GenericList<string> list2 = new GenericList<string>();
GenericList<bool> list3 = new GenericList<string>();
```

Dit kan ook met zelf gemaakt classes:
```cs
public class Person
{
    public Person(string name)
    {
        Name = name;
    }

    public string Name { get; }

    public override string ToString()
    {
        return Name;
    }
}

GenericList<Person> list1 = new GenericList<Person>();
```

In iedere van instantie van `GenericList<T>` wordt elk exemplaar van T in de klasse tijdens runtime vervangen door het argument type. Door middel van deze vervanging hebben we drie afzonderlijke type-safe en efficiënte objecten gemaakt met behulp van enkele klasse definitie.

Op de type-parameters zitten wel enkele constraints. Deze constraints informeren de compiler over de mogelijk type argumenten die meegegeven kunnen worden. Zonder de constraint kan het namelijk elk type zijn. De compiler kan er alleen van uitgaan dat het een member is van het type System.Object, dat is de ultieme basisklasse voor elk .NET type. Als de code de klasse probeert te instantiëren met een type dat niet toegestaan is door een beperking, zal het resultaat een compilatiefout zijn. 

Generieke klassen bevatten bewerkingen die niet specifiek zijn voor een bepaald gegevenstype. Het meest gebruikelijke gebruik voor generieke klassen is met collecties zoals gekoppelde lijsten, hashtabellen, stapels, wachtrijen, bomen, enzovoort. Bewerkingen zoals het toevoegen en verwijderen van items uit de verzameling worden in principe op dezelfde manier uitgevoerd, ongeacht het type gegevens dat wordt opgeslagen.

Het is vaak handig om interfaces te definiëren voor generic collections of voor de generic classes die items in de collection vertegenwoordigen. De voorkeur voor generic classes is om generic interfaces te gebruiken, zoals `IComparable<T>` in plaats van `IComparable`, om box- en unboxing-bewerkingen op waardetypen te voorkomen. De .NET Framework-klassenbibliotheek definieert verschillende generieke interfaces voor gebruik met de collectieklassen in de naamruimte `System.Collections.Generic`.

Een generieke methode is een methode die gedeclareerd wordt met type parameters, dit gaat als volgt: 

```cs
static void Swap<T>(ref T lhs, ref T rhs)
{
    T temp;
    temp = lhs;
    lhs = rhs;
    rhs = temp;
}
```

Het volgende codevoorbeeld laat zien hoe je deze methode kan oproepen met `int` als type argument:

```cs
public static void TestSwap()
{
    int a = 1;
    int b = 2;

    Swap<int>(ref a, ref b);
    System.Console.WriteLine(a + " " + b);
}
```

Dezelfde regels voor het type inferentie zijn van toepassing op statische en instantiemethoden. De compiler kan het type parameters afleiden op basis van de methode argumenten die je doorgeeft. De compiler kan het type paramaters niet alleen afleiden uit een beperking of geretourneerde waarde. Daarom werkt type inferentie niet met methoden die geen parameters hebben. Type inferentie vindt plaats tijdens het compileren voordat de compiler probeert “overloaded method signatures” op te lossen. De compiler past logica van het type inferentie toe op alle generieke methoden die dezelfde naam hebben. In de stap voor het oplossen van overbelasting bevat de compiler alleen die generieke methoden waarop type inferentie is geslaagd.

In C # 2.0 en hoger implementeren eendimensionale arrays met een ondergrens van nul automatisch `IList <T>`. Hiermee kan je generieke methoden maken die dezelfde code kunnen gebruiken om door arrays en andere soorten verzamelingen te bladeren. Deze techniek is vooral nuttig voor het lezen van gegevens in verzamelingen. De interface van `IList <T>` kan niet worden gebruikt om elementen aan een array toe te voegen of te verwijderen. Er wordt een uitzondering gegenereerd als je in deze context een `IList <T>`-methode zoals` RemoveAt` op een array probeert aan te roepen.


Het volgende codevoorbeeld laat zien hoe een enkele generieke methode die een invoerparameter IList <T> gebruikt, zowel een lijst als een array kan doorlopen, in dit geval een array met gehele getallen.

```cs
class Program
{
    static void Main()
    {
        int[] arr = { 0, 1, 2, 3, 4 };
        List<int> list = new List<int>();

        for (int x = 5; x < 10; x++)
        {
            list.Add(x);
        }

        ProcessItems<int>(arr);
        ProcessItems<int>(list);
    }

    static void ProcessItems<T>(IList<T> coll)
    {
        // IsReadOnly returns True for the array and False for the List.
        System.Console.WriteLine
            ("IsReadOnly returns {0} for this collection.",
            coll.IsReadOnly);

        // The following statement causes a run-time exception for the 
        // array, but not for the List.
        //coll.RemoveAt(4);

        foreach (T item in coll)
        {
            System.Console.Write(item.ToString() + " ");
        }
        System.Console.WriteLine();
    }
}
```

Een delegate kan zijn eigen typeparameters definiëren. Code die verwijst naar een generieke delegate kan het type argument specificeren om een gesloten geconstrueerd type te maken, zoals getoond in het volgende voorbeeld:

```cs
public delegate void Del<T>(T item);
public static void Notify(int i) { }

Del<int> m1 = new Del<int>(Notify);
```

C# 2.0 heeft een nieuwe functie genaamd methode groep conversie, die van toepassing is op zowel concrete als generieke delegate typen, waarmee je de vorige regel kunt schrijven met deze simpelere syntax: 

```cs
Del<int> m2 = Notify;
```

Delegates die binnen een generieke klasse zijn gedefinieerd, kunnen de parameters van het generic klasse type op deze manier gebruiken als klasse methoden.

```cs
class Stack<T>
{
    T[] items;
    int index;

    public delegate void StackDelegate(T[] items);
}
```

Code die verwijst naar de delegate moet het type argument van de bevattende klasse als volgt specificeren:

```cs
private static void DoWork(float[] items) { }

public static void TestStack()
{
    Stack<float> s = new Stack<float>();
    Stack<float>.StackDelegate d = DoWork;
}
```

### Bronnen 

- https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/generics/
- https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/generics/generic-type-parameters
- https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/generics/constraints-on-type-parameters
- https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/generics/generic-classes
- https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/generics/generic-interfaces
- https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/generics/generic-methods
- https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/generics/generics-and-arrays
- https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/generics/generic-delegates

## Boxing & unboxing
Boxing is het proces waarbij een waardetype wordt geconverteerd naar het type object of naar elk interfacetype dat door dit waardetype wordt geïmplementeerd. Wanneer de CLR een waardetype invoert, wordt de waarde binnen een `System.Object` instantie geplaatst en opgeslagen op de beheerde heap. Unboxing haalt het waardetype uit het object. Boxing is impliciet; unboxing is expliciet. Het concept van boxing en unboxing ligt ten grondslag aan de uniforme weergave C# van het type systeem waarin een waarde van elk type als een object kan worden behandeld.

### Boxing
Boxing wordt gebruikt om waardetypes op te slaan in de garbage-collected heap. Boxing is een impliciete conversie van een waardetype naar het type object of naar elk interfacetype dat door dit waardetype wordt geïmplementeerd. Het in dozen doen van een waardetype wijst een objectinstantie toe aan de heap en kopieert de waarde naar het nieuwe object.

In het volgende voorbeeld wordt de integer variabele `i` *boxed* en toegewezen aan object `o`

```cs
int i = 123;
object o = (object)i;  // explicit boxing
```

Het resultaat is dat het object `o` een referenties heeft met een waarde van `int` op de heap. Het verschil tussen de twee variabelen, `i` en `o`, is te zien in de volgende afbeelding: 

![Boxing](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/types/media/boxing-and-unboxing/boxing-operation-i-o-variables.gif)


### Unboxing 
Unboxing is een expliciete conversie van het type object naar een waardetype of van een interfacetype naar een waardetype dat de interface implementeert. Een unboxing-operatie bestaat uit:

- Het controleren van de objectinstantie om te controleren of het een boxwaarde van het gegeven waardetype is.
- De waarde uit de instantie kopiëren naar de variabele van het waardetype.

In het volgende voorbeeld kan `o` *unboxed* en toegewezen worden aan integer variabele `i`

```cs
int i = 123;      // a value type
object o = i;     // boxing
int j = (int)o;   // unboxing
```

De volgende afbeelding toont zowel boxing- als unboxing-bewerkingen aan:

![Unboxing](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/types/media/boxing-and-unboxing/unboxing-conversion-operation.gif)


### Bronnen
- https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/types/boxing-and-unboxing

## Delegates & Invoke
Een delegate is een type dat verwijst naar methoden met een bepaalde parameter lijst en return type. Wanneer je een delegate instantieert, kan je zijn exemplaar koppelen aan elke methode met een compatibele signature en een return type. Je kan de methode oproepen (of aanroepen) via de delegate instantie.

Delegates worden gebruikt om methoden als argumenten door te geven aan andere methoden. Event handlers zijn niets meer dan methoden die worden aangeroepen via delegates. Je maakt een aangepaste methode en een klasse. Bijvoorbeeld een Windows-besturingselement kan uw methode oproepen wanneer een bepaald event plaatsvindt. Het volgende voorbeeld toont een gedelegeerde verklaring:

```cs
public delegate int PerformCalculation(int x, int y);
```

Elke methode van elke toegankelijke klasse of struct die overeenkomt met het delegate type kan aan de delegate worden toegewezen. De methode kan statisch- of een instantiemethode zijn. Dit maakt het mogelijk om methodeaanroepen programmatisch te wijzigen en nieuwe code in bestaande klassen in te pluggen.

Een delegate is een type dat een methode veilig inkapselt, vergelijkbaar met een function pointer in C en C ++. In tegenstelling tot C function pointer zijn object georiënteerd, type safe en beveiligd. Het type delegate wordt bepaald door de naam van de delegate.

Een delegate object wordt normaal geconstrueerd door de naam van de methode op te geven die de delegate zal inpakken of met een anonieme functie. Nadat een delegate is geïnstantieerd wordt een methodeaanroep aan de delegate doorgegeven aan die methode. De parameters die door de caller aan de delegate zijn doorgegeven, worden doorgegeven aan de methode (en de eventuele retourwaarde van de methode wordt door de delegate aan de beller geretourneerd). Dit staat bekend als het aanroepen van de delegate. Een geïnstantieerde delegate kan worden opgeroepen alsof het de ingepakte methode zelf is. Als voorbeeld:

```cs
// Create a method for a delegate.
public static void DelegateMethod(string message)
{
    System.Console.WriteLine(message);
}

// Instantiate the delegate.
Del handler = DelegateMethod;

// Call the delegate.
handler("Hello World");
```

Delegate types zijn afgeleid van de Delegate klasse in het .NET Framework. Delegate typen zijn verzegeld (ze kunnen niet worden afgeleid) en het is niet mogelijk om aangepaste klassen van delegate af te leiden. Omdat de geïnstantieerde delegate een object is, kan het worden doorgegeven als parameter of worden toegewezen aan een eigenschap. Hiermee kan een methode een delegate als parameter accepteren en de delegate op een later tijdstip aanroepen. Dit staat bekend als een asynchrone callback en is een gebruikelijke methode om een caller op de hoogte te stellen wanneer een lang proces is voltooid. Wanneer een delegate op deze manier wordt gebruikt, heeft de code die de delegate gebruikt geen kennis van de implementatie van de gebruikte methode. De functionaliteit is vergelijkbaar met de ingekapselde interfaces.

Een ander vaak voorkomend gebruik van callbacks is het definiëren van een aangepaste vergelijkingsmethode en het doorgeven van die delegate aan een sorteermethode. Hiermee kan de code van de caller onderdeel worden van het sorteeralgoritme. De volgende voorbeeldmethode gebruikt het `Del` type als parameter:

```cs
public void MethodWithCallback(int param1, int param2, Del callback)
{
    callback("The number is: " + (param1 + param2).ToString());
}
```

Je kan dan de delegate hierboven gemaakt doorgeven aan de methode:

```cs
MethodWithCallback(1, 2, handler);
```

en dan ontvang je de volgende output in de console: 

```cs
The number is: 3
```

Een delegate kan worden gekoppeld aan een benaamde methode. Wanneer je een gemachtigde instantieert met een benaamde methode, wordt de methode als parameter doorgegeven, bijvoorbeeld:

```cs
// Declare a delegate:
delegate void Del(int x);

// Define a named method:
void DoWork(int k) { /* ... */ }

// Instantiate the delegate using the method as a parameter:
Del d = obj.DoWork;
```

Dit wordt genoemd met behulp van een benaamde methode. Delegates gebouwd met een benaamde methode kunnen een statische methode of een instantiemethode inkapselen. Benaamde methoden zijn de enige manier om een delegate in eerdere versies van C # te instantiëren. In een situatie waarin het creëren van een nieuwe methode ongewenste overhead is, kan je met C # een delegate instantiëren en onmiddellijk een codeblok opgeven dat de delegate zal verwerken wanneer deze wordt aangeroepen. Het blok kan een lambda-expressie of een anonieme methode bevatten.

Invoke is een methode binnen windows forms om een delegate uit te voeren op de thread die de onderliggende window control handle heeft. Deze method zit in de namespace `System.Windows.Forms`. Deze methode heeft 2 overloads namelijk de `Invoke(Delegate)` die een speciefoeke delegate runt en de `Invoke(Delegate, Object[])` die een delegate uitvoert d.m.v. een lijst met argumenten. 

### Bronnen
- https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/delegates/
- https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/delegates/using-delegates
- https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/delegates/delegates-with-named-vs-anonymous-methods
- https://docs.microsoft.com/en-us/dotnet/api/system.windows.forms.control.invoke?view=netframework-4.8

## Threading & async
Of je nu ontwikkelt voor computers met één processor of meerdere, je wilt dat uw toepassing de meest responsieve interactie met de gebruiker biedt, zelfs als de toepassing momenteel ander werk doet. Het gebruik van meerdere "threads of execution" is een van de krachtigste manieren om uw toepassing responsief te houden voor de gebruiker en tegelijkertijd gebruik te maken van de processor tussendoor of zelfs tijdens user events. Met multithreading kan je de responsiviteit van uw applicatie verhogen en, als uw applicatie op een multiprocessor of multi-core systeem draait, de doorvoer verhogen.

Om dit concept beter te kunnen begrijpen gaan we eerst kijken naar het verschil tussen een proces en een thread. Een proces is een uitvoerend programma. Een besturingssysteem gebruikt processen om de applicaties die worden uitgevoerd te scheiden. Een thread is de basiseenheid waaraan een besturingssysteem processortijd toewijst. Elke thread heeft een planningsprioriteit en onderhoudt een set structuren die het systeem gebruikt om de threadcontext op te slaan wanneer de uitvoering van de thread wordt gepauzeerd. De threadcontext bevat alle informatie die de thread nodig heeft om de uitvoering te hervatten, inclusief de set CPU-registers en de stack van de thread. Meerdere threads kunnen worden uitgevoerd in de context van een proces. Alle threads van een proces delen zijn virtuele adresruimte. Een thread kan elk deel van de programmacode uitvoeren, inclusief delen die momenteel door een andere thread worden uitgevoerd. Standaard wordt een .NET-programma gestart met een enkele thread, vaak de primaire thread genoemd. Het kan echter extra threads maken om code parallel of gelijktijdig met de primaire thread uit te voeren. Deze threads worden vaak "worker threads" genoemd.

Je gebruikt meerdere threads om de responsiviteit van uw applicatie te vergroten en om te profiteren van een multiprocessor of multi-core systeem om de doorvoer van de applicatie te verhogen. Overweeg een desktoptoepassing, waarbij de primaire thread verantwoordelijk is voor gebruikersinterface-elementen en reageert op gebruikersacties. Gebruik werkthreads om tijdrovende bewerkingen uit te voeren die anders de primaire thread bezetten en de gebruikersinterface niet reageren. Je kan ook een speciale thread gebruiken voor netwerk- of apparaatcommunicatie om sneller te reageren op inkomende berichten of gebeurtenissen.

Vanaf .NET Framework versie 2.0, laat de common language runtime de meeste unhandled exceptions in threads op natuurlijke wijze verlopen. In de meeste gevallen betekent dit dat de unhandled exception ertoe leidt dat de toepassing wordt beëindigd.

De common language runtime biedt een backstop voor bepaalde unhandled exceptions die worden gebruikt voor het regelen van de program flow:
- Een ThreadAbortException wordt in een thread gegooid omdat Abort werd aangeroepen.
- Een AppDomainUnloadedException wordt in een thread gegooid omdat het toepassingsdomein waarin de thread wordt uitgevoerd, wordt verwijderd.
- De algemene runtime of een hostproces beëindigt de thread door een interne exception te genereren.
Als een van deze exceptions niet wordt verwerkt in threads die zijn gemaakt door de common language runtime, wordt de thread beëindigd door de uitzondering, maar de common language runtime staat niet toe dat de uitzondering verder gaat.
Als deze exceptuions niet worden verwerkt in de hoofdthread of in threads die de runtime zijn ingevoerd vanuit een onbeheerde code, verlopen ze normaal en wordt de toepassing beëindigd.

Wanneer threads stil kunnen mislukken, zonder de toepassing te beëindigen, kunnen ernstige programmeerproblemen onopgemerkt blijven. Dit is met name een probleem voor services en andere applicaties die langere tijd worden gebruikt. Als threads mislukken, wordt de programmastatus geleidelijk beschadigd. De prestaties van de toepassing kunnen verslechteren of de toepassing reageert mogelijk niet meer.

Wanneer meerdere threads de eigenschappen en methoden van een enkel object kunnen aanroepen, is het van cruciaal belang dat deze calls worden gesynchroniseerd. Anders kan een thread onderbreken wat een andere thread doet en kan het object in een ongeldige status blijven. Een klasse waarvan de leden tegen dergelijke onderbrekingen worden beschermd, wordt thread-safe genoemd.

NET biedt verschillende strategieën om de toegang tot subsystemen en statische leden te synchroniseren:
- Gesynchroniseerde codegebieden. Je kan de Monitor-klasse of compilerondersteuning voor deze klasse gebruiken om alleen het codeblok te synchroniseren dat dit nodig heeft, waardoor de prestaties worden verbeterd.
- Handmatige synchronisatie. Je kan de synchronisatieobjecten gebruiken die worden aangeboden door de .NET-klassenbibliotheek.
- Gesynchroniseerde contexten. Voor .NET Framework- en Xamarin-toepassingen kan je het SynchronizationAttribute gebruiken om eenvoudige, automatische synchronisatie voor ContextBoundObject-objecten in te schakelen.
- Collectieklassen in de namespace System.Collections.Concurrent. Deze klassen bieden ingebouwde gesynchroniseerde toevoeg- en verwijderbewerkingen.

Een beheerde thread is een achtergrondthread of een voorgrondthread. Achtergrondthreads zijn identiek aan voorgrondthreads met één uitzondering: een achtergrondthread houdt de beheerde uitvoeringsomgeving niet actief. Nadat alle voorgrondthreads in een beheerd proces zijn gestopt, stopt het systeem alle achtergrondthreads en wordt het afgesloten.

Als alternatief op threads is er ook async programming. Hiermee kan je hetzelfde resultaat bereiken en behoud je de responsiveness van de applicatie. Traditionele technieken voor het schrijven van asynchrone toepassingen kunnen echter ingewikkeld zijn, waardoor ze moeilijk te schrijven, te debuggen en te onderhouden zijn.

C# 5 introduceerde een vereenvoudigde aanpak, async programmeren, die gebruik maakt van asynchrone ondersteuning in .NET Framework 4.5 (en hoger), .NET Core en Windows Runtime. De compiler doet het moeilijke werk dat de ontwikkelaar deed en uw toepassing heeft een logische structuur die lijkt op synchrone code. Als resultaat krijg je alle voordelen van asynchroon programmeren met een fractie van de inspanning.

Async-methoden zijn bedoeld als niet-blokkerende bewerkingen. Een wachtende uitdrukking in een asynchrone methode blokkeert de huidige thread niet terwijl de verwachte taak wordt uitgevoerd. In plaats daarvan meldt de expressie de rest van de methode als een voortzetting en retourneert de besturing aan de beller van de async-methode.

De asynchrone en wachtende zoekwoorden veroorzaken geen extra threads. Async-methoden vereisen geen multithreading omdat een async-methode niet op zijn eigen thread draait. De methode wordt uitgevoerd in de huidige synchronisatiecontext en gebruikt alleen tijd op de thread wanneer de methode actief is. Je kan Task.Run gebruiken om CPU-gebonden werk naar een achtergrondthread te verplaatsen, maar een achtergrondthread helpt niet bij een proces dat wacht op het beschikbaar komen van resultaten.

De asynchrone benadering van asynchrone programmering verdient in bijna alle gevallen de voorkeur boven bestaande benaderingen. In het bijzonder is deze aanpak beter dan de BackgroundWorker-klasse voor I/O-gebonden operaties omdat de code eenvoudiger is en je niet hoeft te waken tegen raceomstandigheden. In combinatie met de Task.Run-methode is async-programmering beter dan BackgroundWorker voor CPU-gebonden bewerkingen omdat async-programmering de coördinatiegegevens van het uitvoeren van uw code scheidt van het werk dat Task.Run overbrengt naar de threadpool.

### Bronnen
- https://docs.microsoft.com/en-us/dotnet/standard/threading/
- https://docs.microsoft.com/en-us/dotnet/standard/threading/threads-and-threading
- https://docs.microsoft.com/en-us/dotnet/standard/threading/exceptions-in-managed-threads
- https://docs.microsoft.com/en-us/dotnet/standard/threading/synchronizing-data-for-multithreading
- https://docs.microsoft.com/en-us/dotnet/standard/threading/foreground-and-background-threads
- https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/async/task-asynchronous-programming-model