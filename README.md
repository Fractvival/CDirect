# CDirect 
## COM port director

Aplikace sloužící pro propojení skeneru a tiskárny pomocí sériového portu (COM portu).
Záměrem této aplikace je, aby data načtená ze skeneru čárových kódů poslala na tiskárnu čárových kódů a to pomocí COM portů.

Při prvním spuštění dojde k vygenerování souboru s nastavením, setting.txt.
Aplikace se hned snaží rozjet na defaultní nastavení, takže ji po prvním spuštění zavřeme a provedeme nastavení.

Soubor s nastavením se nachází ve stejné složce jako samotný program (exe soubor).

DOPORUČUJI editovat tento texťák pomocí programu Notepad++

V souboru editujeme pouze hodnoty, texty s lomítky musí zůstat tak jak jsou, vše má svoji pevně danou pozici!
V nastavení je tedy možné provést tyto možnosti:

- Nastavení parametrů COM portů (jak skeneru, tak tiskárny, každému zvlášť)
- Nastavení minimální/maximální délky čárového kódu (ostatní délky tudíž budou vyřazeny)
- Nastavení prefixu/sufixu čárového kódu (tedy povely pro tiskárnu)
- Nastavení času automatického ukládání počtu vytisknutých kódů (tedy počty těch nevyřazených co skutečny odešly na tiskárnu)
- Možnost časově automatického restartu aplikace (tato možnost vyřadí automatické ukládání počtů a provede jejich uložení před restartem)

Současně s vytvořením souboru s nastavením se vytvoří i soubor s počty již vytisknutých kódů, soubor packet.txt.
V tomto souboru je pouze hodnota počtu kódů, tedy jeden řádek s hodnotou. Ta je před spuštěním aplikace načtena a navyšuje se o nové v průběhu běhu aplikace.

Nástroje pro testování této aplikace:

https://sourceforge.net/projects/com0com/

https://sourceforge.net/projects/readwriteserial/



