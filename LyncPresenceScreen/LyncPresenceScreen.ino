// Paint application - Demonstate both TFT and Touch Screen
//  This library is free software; you can redistribute it and/or
//  modify it under the terms of the GNU Lesser General Public
//  License as published by the Free Software Foundation; either
//  version 2.1 of the License, or (at your option) any later version.
//
//  This library is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//  Lesser General Public License for more details.
//
//  You should have received a copy of the GNU Lesser General Public
//  License along with this library; if not, write to the Free Software
//  Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
#include <stdint.h>
#include <TouchScreen.h> 
#include <TFT.h>

#ifdef SEEEDUINO
#define YP A2   // must be an analog pin, use "An" notation!
#define XM A1   // must be an analog pin, use "An" notation!
#define YM 14   // can be a digital pin, this is A0
#define XP 17   // can be a digital pin, this is A3 
#endif

//#define DEBUG

#define TS_MINX 140 
#define TS_MAXX 900
#define TS_MINY 120
#define TS_MAXY 940

#define FREE 1
#define BUSY 2
#define AWAY 3
#define DND 4


int color = WHITE;  //Paint brush color

// For better pressure precision, we need to know the resistance
// between X+ and X- Use any multimeter to read it
// The 2.8" TFT Touch shield has 300 ohms across the X plate

TouchScreen ts = TouchScreen(XP, YP, XM, YM, 300); //init TouchScreen port pins
int currentStatus = FREE;
int hitStatus = 0;

// setup everything
void setup()
{
  Serial.begin(9600);
  Tft.init();  //init TFT library
  pinMode(0,OUTPUT);
  // draw white background
  Tft.fillRectangle(0, 0, 240,320,BLACK);
  //Draw the indicators, all are off
  drawFree(currentStatus==FREE?true:false);
  drawBusy(currentStatus==BUSY?true:false);
  drawAway(currentStatus==AWAY?true:false);
  drawDND(currentStatus==DND?true:false);
}

// FREE
void drawFree(bool isActive) {
  Tft.fillCircle(55, 238, 50, GREEN); // Free
  if(!isActive) {
    Tft.fillCircle(55, 238, 40, WHITE); // Free
  }
}

// BUSY
void drawBusy(bool isActive) {
  Tft.fillCircle(179, 238, 50, RED); // Busy
  if(!isActive) {
    Tft.fillCircle(179, 238, 40, WHITE); // Busy
  }
}

// AWAY
void drawAway(bool isActive) {
  Tft.fillCircle(179, 83, 50, YELLOW); // Away
  if(!isActive) {
    Tft.fillCircle(179, 83, 40, WHITE); // Away
  }
}

// DND
void drawDND(bool isActive) {
  Tft.fillCircle(55, 83, 50, RED); // DND
  if(!isActive) {
    Tft.fillCircle(55, 83, 40, WHITE); // DND
  }
  Tft.fillRectangle(40, 25, 30, 117, RED);
  Tft.fillRectangle(43, 28, 24, 111, WHITE);
}

// determines what status icon was pressed
int getStatus(int x, int y) {
  if(x > 5 && x < 105) {
    // means press in on area of FREE and DND
    if(y > 188 && y < 288) {      
      return FREE;
    } 
    else if (y > 33 && y < 133) {
      return DND;
    } 
  }
  else if(x > 129 && x < 229) {
    if(y > 188 && y < 288) {
      return BUSY;
    } 
    else if (y > 33 && y < 133) {
      return AWAY;
    }
  }
  return 0;
}

// sets sprite and send request to change status to Serial
void setStatus() {
  String statusMessage;
  switch(currentStatus) {
  case FREE :
    drawFree(true);
    statusMessage="[FREE]";
    break;
  case BUSY :
    drawBusy(true);
    statusMessage="[BUSY]";
    break;
  case AWAY :
    drawAway(true);
    statusMessage="[AWAY]";
    break;
  case DND :
    drawDND(true);
    statusMessage="[DND]";
    break;
  default :
    break;
  }
  Serial.print(statusMessage);
}

void loop()
{
  // a point object holds x y and z coordinates.
  Point p = ts.getPoint();
  //map the ADC value read to into pixel co-ordinates
  p.x = map(p.x, TS_MINX, TS_MAXX, 240, 0);
  p.y = map(p.y, TS_MINY, TS_MAXY, 320, 0);
  // we have some minimum pressure we consider 'valid'
  // pressure of 0 means no pressing!
  if (p.z > ts.pressureThreshhold) {
#ifdef DEBUG
    Serial.print("X = "); 
    Serial.print(p.x);
    Serial.print("\tY = "); 
    Serial.print(p.y);
    Serial.print("\tPressure = "); 
    Serial.println(p.z);
#endif
    // Determine what status was pressed
    hitStatus = getStatus(p.x, p.y);
    if(hitStatus != 0) {
      // clear the active status
      switch(currentStatus) {
      case FREE :
        drawFree(false);
        break;
      case BUSY :
        drawBusy(false);
        break;
      case AWAY :
        drawAway(false);
        break;
      case DND :
        drawDND(false);
        break;
      default :
        break;
      }
      // then set the current status
      currentStatus = hitStatus;
      setStatus();
      // delay for debounce
      delay(1000);
    }
  }
}


