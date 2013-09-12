//This program is free software: you can redistribute it and/or modify
//it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or
//(at your option) any later version.

//This program is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//GNU General Public License for more details.

#ifndef APIQUERY_H
#define APIQUERY_H

#include <QString>
#include <QtNetwork/QtNetwork>
#include <QUrl>
#include "core.h"
#include "exception.h"
#include "query.h"

enum Format
{
    XML,
    JSON,
    PlainText,
    Default
};

class ApiQuery : Query
{
private:
    QNetworkAccessManager NetworkManager;
    void ConstructUrl();
public:
    ApiQuery();
    //! This is a requested format in which the result should be written in
    Format RequestFormat;
    QString URL;
    void Process();
};

#endif // APIQUERY_H
