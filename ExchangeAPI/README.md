# FAQ

## Какие запросы можно делать?
| Запрос                                                                     | Описание                                     |
|----------------------------------------------------------------------------|----------------------------------------------|
| GET /latest?base=EUR&symbols=RUB,USD                                       | Получить последний доступный курс валют      |
| GET /{date}?base=EUR&symbols=RUB,USD                                       | Получить курс валют на указанную дату        |
| GET /history?base=EUR&symbols=RUB,USD&start_at=2008-01-1&end_at=2020-01-01 | Получить все доступные курсы валют в периоде |

## Какие параметры у запросов обязательные? Какой их формат? Какие значения по-умолчанию?
| Название | Тип параметра | Допустимые значения       | Описание                                          | Значение по-умолчанию      |
|----------|---------------|---------------------------|---------------------------------------------------|----------------------------|
| base     | query         | Символ                    | Валюта, относительно которой будет считаться курс | EUR                        |
| symbols  | query         | Символы через запятую     | Валюты, курс которых будет показан                | Все валюты                 |
| date     | URL           | Дата в формате yyyy-MM-dd | Дата, на которую будет найден курс                | НЕТ. Параметр обязательный |
| start_at | query         | Дата в формате yyyy-MM-dd | Начало пероида поиска курсов (включительно)       | НЕТ. Параметр обязательный |
| end_at   | query         | Дата в формате yyyy-MM-dd | Конец пероида поиска курсов (включительно)        | НЕТ. Параметр обязательный |


## Как выбрать тип базы данных при запуске?
В appsettings.json параметр `Database:Type`
`sqlite` - для использования Sqlite
`sqlserver` - для использование MS Sql Server
Например:
```json
{
  "Database": {
    "Type": "sqlite",
    "ConnectionString": "Data Source=E:\\exchange.db"
  }
}
```
## Как инициализировать базу данных?
Для Sqlite: 
```sql
CREATE TABLE "Rates" (
    "Date" INTEGER NOT NULL,
    "Currency" TEXT NOT NULL,
    "Rate" TEXT NOT NULL,
    CONSTRAINT "PK_Rates" PRIMARY KEY ("Date", "Currency")
);


CREATE INDEX "IX_Rates_Currency" ON "Rates" ("Currency");


CREATE INDEX "IX_Rates_Date_Currency" ON "Rates" ("Date", "Currency");
```
Для SqlServer

```sql
CREATE TABLE [Rates] (
    [Date] datetimeoffset NOT NULL,
    [Currency] nvarchar(3) NOT NULL,
    [Rate] decimal(18,2) NOT NULL,
    CONSTRAINT [PK_Rates] PRIMARY KEY ([Date], [Currency])
);
GO


CREATE INDEX [IX_Rates_Currency] ON [Rates] ([Currency]);
GO


CREATE INDEX [IX_Rates_Date_Currency] ON [Rates] ([Date], [Currency]);
GO
```
## Как изменить урлы для скачивания данных?
В appsettings.json
```json
{
  "ReferenceRates": {
    "HistoricalDataUri": "https://www.ecb.europa.eu/stats/eurofxref/eurofxref-hist.xml",
    "FreshDataUri": "https://www.ecb.europa.eu/stats/eurofxref/eurofxref-hist-90d.xml"
  }
}
```
## Как загрузить начальные данные перед первым запуском?
Запустить проект ExchangeRateLoader.CLI с параметром init

```
cd ../ExchangeRateLoader.CLI
dotnet run -- init
```
