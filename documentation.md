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

TODO: gebleven bij generic methods.


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
Een delegate is een type dat verwijst naar methoden met een bepaalde parameter lijst en return type. Wanneer u een delegate instantieert, kunt u zijn exemplaar koppelen aan elke methode met een compatibele signature en een return type. U kunt de methode oproepen (of aanroepen) via de delegate instantie.

### Bronnen
- https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/delegates/
- https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/delegates/using-delegates
- https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/delegates/delegates-with-named-vs-anonymous-methods
- https://docs.microsoft.com/en-us/dotnet/api/system.windows.forms.control.invoke?view=netframework-4.8

## Threading & async
Of u nu ontwikkelt voor computers met één processor of meerdere, u wilt dat uw toepassing de meest responsieve interactie met de gebruiker biedt, zelfs als de toepassing momenteel ander werk doet. Het gebruik van meerdere uitvoeringsdraden is een van de krachtigste manieren om uw toepassing responsief te houden voor de gebruiker en tegelijkertijd gebruik te maken van de processor tussendoor of zelfs tijdens gebruikersevenementen. Hoewel dit gedeelte de basisconcepten van threading introduceert, richt het zich op managed threading-concepten en het gebruik van managed threading.

### Bronnen
- https://docs.microsoft.com/en-us/dotnet/standard/threading/
- https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/async/