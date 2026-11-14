<p align="center">
  <img src="Galaxy Buds App/Assets/SquareTile150x150.png" width="128" alt="Icona Galaxy Buds Client">
</p>

<h1 align="center">Galaxy Buds Client per Windows Phone</h1>

<p align="center">
  <strong>Gestore Galaxy Buds non ufficiale per Windows Phone 8.1 Silverlight.</strong><br>
  Connetti i tuoi Galaxy Buds via Bluetooth e controlla audio, touch e impostazioni dal tuo Windows Phone.
</p>

<p align="center">
  <a href="#funzionalita">Funzionalità</a> ·
  <a href="#installazione">Installazione</a> ·
  <a href="#compilazione">Compilazione</a> ·
  <a href="#utilizzo">Utilizzo</a> ·
  <a href="#crediti">Crediti</a> ·
  <a href="#licenza">Licenza</a>
</p>

<p align="center">
  <a href="LICENSE"><img src="https://img.shields.io/badge/licenza-GPL--3.0-blue" alt="GPL-3.0"></a>
  <img src="https://img.shields.io/badge/piattaforma-Windows%20Phone%208.1-0078D7" alt="Windows Phone 8.1">
  <img src="https://img.shields.io/badge/lingue-Italiano%20%7C%20Inglese-green" alt="Lingue">
</p>

Galaxy Buds Client si connette ai Samsung Galaxy Buds associati via Bluetooth RFCOMM e permette di leggere lo stato della batteria, regolare l'equalizzatore, controllare il suono ambientale, bloccare il touchpad e trovare auricolari smarriti. Funziona su Windows Phone 8.1 Silverlight ed è software libero.

**Utile per:** leggere la batteria in tempo reale, cambiare i preset equalizzatore, bloccare il touchpad, trovare auricolari nelle vicinanze.

**Non adatto per:** aggiornamenti firmware (disattivati per complessità di sicurezza), funzioni desktop come integrazione system tray o routing audio spaziale.

## Funzionalità

| Funzione | Stato |
|---|---|
| Ricerca e connessione dispositivi | Funzionante |
| Batteria sinistra/destra in tempo reale | Funzionante |
| Preset equalizzatore (5 bande) | Funzionante |
| Suono ambientale on/off | Funzionante |
| Controllo volume ambientale | Funzionante |
| Blocco touchpad | Funzionante |
| Trova i miei auricolari | Funzionante |
| Reset di fabbrica (con conferma) | Funzionante |
| Batteria custodia | Supportato da Buds+ in poi |
| Equalizzatore personalizzato | Supportato da Buds2 Pro in poi |
| ANC / Controllo rumore | Supportato da Buds Live in poi |
| Aggiornamento firmware | Non attivo su WP |

## Modelli supportati

Galaxy Buds (2019), Galaxy Buds+, Galaxy Buds Live, Galaxy Buds Pro, Galaxy Buds2, Galaxy Buds2 Pro, Galaxy Buds FE, Galaxy Buds Core, Galaxy Buds3, Galaxy Buds3 Pro, Galaxy Buds3 FE, Galaxy Buds4, Galaxy Buds4 Pro.

La disponibilità delle funzioni varia per modello e revisione firmware. Vedi [docs/FEATURE-PARITY.md](docs/FEATURE-PARITY.md) per la matrice completa.

## Installazione

Compila lo XAP dal sorgente (vedi [Compilazione](#compilazione)) e installa su un dispositivo Windows Phone 8.1 usando lo strumento di distribuzione applicazioni Windows Phone o `AppDeployCmd`.

**Associa prima i tuoi Galaxy Buds** nelle impostazioni Bluetooth del telefono, poi apri l'app e premi "Cerca auricolari".

## Compilazione

**Prerequisiti:** Visual Studio 2013 o MSBuild 14 con SDK Windows Phone 8.1.

```powershell
MSBuild "Galaxy Buds App.sln" /p:Configuration=Release
```

## Utilizzo

1. Associa i Galaxy Buds nelle impostazioni Bluetooth del telefono.
2. Apri Galaxy Buds Client.
3. Premi **Cerca auricolari** e seleziona il tuo dispositivo.
4. Premi **Connetti**. Le letture batteria appaiono nella panoramica.
5. Usa le sezioni Audio, Controlli e Strumenti per gestire i tuoi auricolari.

L'app segue la lingua del telefono: italiano sui dispositivi italiani, inglese ovunque altro.

## Privacy

L'app non invia dati a servizi esterni. La comunicazione Bluetooth è solo locale. Nessuna analitica, nessun tracciamento.

## Crediti

Questo progetto è basato su [GalaxyBudsClient](https://github.com/timschneeb/GalaxyBudsClient) di Tim Schneeberger (`@timschneeb`), che documenta il protocollo RFCOMM proprietario Samsung Galaxy Buds tramite reverse engineering. Note di protocollo e implementazione sono usate sotto GPL-3.0. Vedi [docs/PROTOCOL-CREDITS.md](docs/PROTOCOL-CREDITS.md) per l'attribuzione completa.

Questo port per Windows Phone è indipendente e non ufficiale. Non è affiliato con Samsung né con l'autore originale.

## Licenza

GPL-3.0. Vedi [LICENSE](LICENSE).

Galaxy Buds e Samsung sono marchi di Samsung Electronics Co., Ltd.
