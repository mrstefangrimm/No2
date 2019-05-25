/* SoftDKb2.pde - Dedicated Keyboard Processing 3.0 software for the No2 (C) motion phantom  //<>//
 * Copyright (C) 2019 by Stefan Grimm
 *
 * This is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This software is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser General Public License for more details.

 * You should have received a copy of the GNU Lesser General Public License
 * along with the SoftDKb software.  If not, see
 * <http://www.gnu.org/licenses/>.
 */

import processing.serial.*;

int NUMSERVOS = 6;
Serial comPort;
String sendBuffer = new String("0x0 0000 0000 0000 0000 0000 0000 0000 0000");
int[] recvBuffer = new int[10];
int readingPos = 0;
boolean receiving;
boolean synced = false;

int[] positionBuffer = new int[NUMSERVOS];
boolean led1 = true;
boolean led2 = false;
boolean led3 = false;
boolean led4 = false;
int freeMemory = 0;

void setup() {
  
  size(900, 800);
  
  // Debug: printArray(PFont.list());
  PFont font = createFont(PFont.list()[2], 34);
  textFont(font);
  
  // Get all the available serial ports:
  String[] comPorts = Serial.list();
  if (comPorts != null && comPorts.length > 0) {
    // Debug: printArray(comPorts);
    String portName = Serial.list()[0];
    println(portName);
    comPort = new Serial(this, portName, 9600);
  }
}

void draw() {
  background(0);
  
  // Output LEDs
  if (led1) fill(color(0, 255, 0));
  else fill(200);
  rect(600, 150, 40, 40);
  if (led2) fill(color(0, 0, 255));
  else fill(200);
  rect(650, 150, 40, 40);
  if (led3) fill(color(255, 0, 0));
  else fill(200);
  rect(700, 150, 40, 40);
  if (led4) fill(color(255, 255, 0));
  else fill(200);
  rect(700, 200, 40, 40);
  
  // Function keys
  fill(200);
  rect(50, 500, 40, 40);
  rect(50, 550, 40, 40);
  rect(50, 600, 40, 40);
  rect(100, 600, 40, 40);
  
  // Program keys
  rect(100, 550, 40, 40);
  rect(150, 550, 40, 40);
  rect(200, 550, 40, 40);
  rect(250, 550, 40, 40);
  rect(300, 550, 40, 40);
  rect(350, 550, 40, 40);
  rect(400, 550, 40, 40);
  rect(450, 550, 40, 40);
  
  // left lower
  rect(150, 250, 40, 40);
  rect(100, 300, 40, 40);
  rect(200, 300, 40, 40);
  rect(150, 350, 40, 40);
    
  // right lower
  rect(350, 250, 40, 40);
  rect(300, 300, 40, 40);
  rect(400, 300, 40, 40);
  rect(350, 350, 40, 40);
  
  // Gating
  rect(550, 250, 40, 40);
  rect(500, 300, 40, 40);
  rect(600, 300, 40, 40);
  rect(550, 350, 40, 40);
  
  text("Last Sent:", 10, 50);
  text(sendBuffer, 200, 50);
  text("Last Received:", 10, 100);
  for (int n=0; n<10; n++) {
    text((char)recvBuffer[n], 200 + 50*n, 100);
  }
  for (int n=0; n<NUMSERVOS; n++) {
    text(n, 700, 300 + n*50);
    text(positionBuffer[n], 800, 300 + n*50);
  }

  text("Free Memory:", 10, 750);
  text(freeMemory, 200, 750);
  rect(270, 715, 40, 40);
}

public int getReceivedNumber() {
  int pos = 0;
  int pot = 0;
  for (int n=readingPos-1; n > 0; n--, pot++) {
    int nval = recvBuffer[n] - '0';
    int fac = (int)pow(10, pot);
    pos += nval * fac;
  }
  return pos;
}

void serialEvent(Serial myPort) {
  
  int val = myPort.read();
  if (val == -1) return;
  
  if (synced == false) {
    print((char)val);
    
    if (readingPos == 6) {
      for (int n=0; n < 6; n++) {
        recvBuffer[n] = recvBuffer[n+1];
      }
      recvBuffer[6] = val;
      if (recvBuffer[0] == (int)'S' && recvBuffer[1] == (int)'y' && recvBuffer[2] == (int)'n' &&
          recvBuffer[3] == (int)'c' && recvBuffer[4] == (int)'e' && recvBuffer[5] == (int)'d') {
        println("synced");
        synced = true;
      }
    }
    else {
      recvBuffer[readingPos] = val;
      readingPos++;
      if (readingPos == 1) {
        comPort.write(0x4);
        sendBuffer = "0x4";
      }
    }    
  }
  else {    
    if (!receiving && val != '|') {
      print((char)val);
    }      
    if (val == '|') {
      receiving = !receiving;
      if (receiving) {
        readingPos = 0;
      } 
      else if (recvBuffer[0] == 'G') { led1 = true; led2 = false; led3 = false; led4 = false; }
      else if (recvBuffer[0] == 'H') { led1 = false; led2 = true; led3 = false; led4 = false; }
      else if (recvBuffer[0] == 'I') { led1 = false; led2 = false; led3 = true; led4 = false; }
      else if (recvBuffer[0] == 'J') { led1 = false; led2 = false; led3 = false; led4 = true; }
      else if (recvBuffer[0] == 'K') { freeMemory = getReceivedNumber(); }
      else {
        int motor = recvBuffer[0] - '0';
        if (recvBuffer[0] >= 'A' && recvBuffer[0] <= 'F') {
          motor = 10 + (recvBuffer[0] - 'A');
        }
        if (motor >= 0 && motor < NUMSERVOS) {  
          positionBuffer[motor] = getReceivedNumber();
        }
      }  
    }
    else if (receiving) {
      recvBuffer[readingPos] = val;
      readingPos++;
      if (readingPos >= 10) readingPos = 0;
    }
  }
}

void mousePressed() {
  if (comPort == null) {
    println("No device attached");
    return;
  }
  // Example: (31<<3) | 0x1 = 11111 << 3 | 001 = 11111001
  if      (mousePressedGAL() == true) { comPort.write(( 0<<3) | 0x1); sendBuffer = "0x1 0000 0000 0000 0000 0000 0000 0000 0001"; }
  else if (mousePressedGAT() == true) { comPort.write(( 1<<3) | 0x1); sendBuffer = "0x1 0000 0000 0000 0000 0000 0000 0000 0010"; } 
  else if (mousePressedGAB() == true) { comPort.write(( 2<<3) | 0x1); sendBuffer = "0x1 0000 0000 0000 0000 0000 0000 0000 0100"; }
  else if (mousePressedGAR() == true) { comPort.write(( 3<<3) | 0x1); sendBuffer = "0x1 0000 0000 0000 0000 0000 0000 0000 1000"; }
  else if (mousePressedFP7() == true) { comPort.write(( 4<<3) | 0x1); sendBuffer = "0x1 0000 0000 0000 0000 0000 0000 0001 0000"; } 
  else if (mousePressedFP6() == true) { comPort.write(( 5<<3) | 0x1); sendBuffer = "0x1 0000 0000 0000 0000 0000 0000 0010 0000"; }
  else if (mousePressedFP5() == true) { comPort.write(( 6<<3) | 0x1); sendBuffer = "0x1 0000 0000 0000 0000 0000 0000 0100 0000"; }
  else if (mousePressedFP8() == true) { comPort.write(( 7<<3) | 0x1); sendBuffer = "0x1 0000 0000 0000 0000 0000 0000 1000 0000"; }  
  else if (mousePressedRLB() == true) { comPort.write(( 8<<3) | 0x1); sendBuffer = "0x1 0000 0000 0000 0000 0000 0001 0000 0000"; }
  else if (mousePressedRLR() == true) { comPort.write(( 9<<3) | 0x1); sendBuffer = "0x1 0000 0000 0000 0000 0000 0010 0000 0000"; } 
  else if (mousePressedRLL() == true) { comPort.write((10<<3) | 0x1); sendBuffer = "0x1 0000 0000 0000 0000 0000 0100 0000 0000"; }
  else if (mousePressedRLT() == true) { comPort.write((11<<3) | 0x1); sendBuffer = "0x1 0000 0000 0000 0000 0000 1000 0000 0000"; }    
  else if (mousePressedFP4() == true) { comPort.write((16<<3) | 0x1); sendBuffer = "0x1 0000 0000 0000 0001 0000 0000 0000 0000"; }
  else if (mousePressedFP3() == true) { comPort.write((17<<3) | 0x1); sendBuffer = "0x1 0000 0000 0000 0010 0000 0000 0000 0000"; } 
  else if (mousePressedFP2() == true) { comPort.write((18<<3) | 0x1); sendBuffer = "0x1 0000 0000 0000 0100 0000 0000 0000 0000"; } 
  else if (mousePressedFP1() == true) { comPort.write((19<<3) | 0x1); sendBuffer = "0x1 0000 0000 0000 1000 0000 0000 0000 0000"; }       
  else if (mousePressedFRM() == true) { comPort.write((20<<3) | 0x1); sendBuffer = "0x1 0000 0000 0001 0000 0000 0000 0000 0000"; } 
  else if (mousePressedFPS() == true) { comPort.write((21<<3) | 0x1); sendBuffer = "0x1 0000 0000 0010 0000 0000 0000 0000 0000"; }   
  else if (mousePressedFMM() == true) { comPort.write((22<<3) | 0x1); sendBuffer = "0x1 0000 0000 0100 0000 0000 0000 0000 0000"; } 
  else if (mousePressedFCA() == true) { comPort.write((23<<3) | 0x1); sendBuffer = "0x1 0000 0000 1000 0000 0000 0000 0000 0000"; }  
  else if (mousePressedLLB() == true) { comPort.write((24<<3) | 0x1); sendBuffer = "0x1 0000 0001 0000 0000 0000 0000 0000 0000"; }
  else if (mousePressedLLR() == true) { comPort.write((25<<3) | 0x1); sendBuffer = "0x1 0000 0010 0000 0000 0000 0000 0000 0000"; } 
  else if (mousePressedLLL() == true) { comPort.write((26<<3) | 0x1); sendBuffer = "0x1 0000 0100 0000 0000 0000 0000 0000 0000"; }
  else if (mousePressedLLT() == true) { comPort.write((27<<3) | 0x1); sendBuffer = "0x1 0000 1000 0000 0000 0000 0000 0000 0000"; } 
  else if (mousePressedFreeMem() == true) { comPort.write(0x3);       sendBuffer = "0x3"; }
}

boolean mousePressedFMM() { return ((mouseX >= 50) && (mouseX <= 90) && (mouseY >= 500) && (mouseY <= 540)); }
boolean mousePressedFPS() { return ((mouseX >= 50) && (mouseX <= 90) && (mouseY >= 550) && (mouseY <= 590)); }
boolean mousePressedFRM() { return ((mouseX >= 50) && (mouseX <= 90) && (mouseY >= 600) && (mouseY <= 640)); }
boolean mousePressedFCA() { return ((mouseX >= 100) && (mouseX <= 140) && (mouseY >= 600) && (mouseY <= 640)); }

boolean mousePressedFP1() { return ((mouseX >= 100) && (mouseX <= 140) && (mouseY >= 550) && (mouseY <= 590)); }
boolean mousePressedFP2() { return ((mouseX >= 150) && (mouseX <= 190) && (mouseY >= 550) && (mouseY <= 590)); }
boolean mousePressedFP3() { return ((mouseX >= 200) && (mouseX <= 240) && (mouseY >= 550) && (mouseY <= 590)); }
boolean mousePressedFP4() { return ((mouseX >= 250) && (mouseX <= 290) && (mouseY >= 550) && (mouseY <= 590)); }
boolean mousePressedFP5() { return ((mouseX >= 300) && (mouseX <= 340) && (mouseY >= 550) && (mouseY <= 590)); }
boolean mousePressedFP6() { return ((mouseX >= 350) && (mouseX <= 390) && (mouseY >= 550) && (mouseY <= 590)); }
boolean mousePressedFP7() { return ((mouseX >= 400) && (mouseX <= 440) && (mouseY >= 550) && (mouseY <= 590)); }
boolean mousePressedFP8() { return ((mouseX >= 450) && (mouseX <= 490) && (mouseY >= 550) && (mouseY <= 590)); }

boolean mousePressedLLT() { return ((mouseX >= 150) && (mouseX <= 190) && (mouseY >= 250) && (mouseY <= 390)); }
boolean mousePressedLLL() { return ((mouseX >= 100) && (mouseX <= 140) && (mouseY >= 300) && (mouseY <= 440)); }
boolean mousePressedLLR() { return ((mouseX >= 200) && (mouseX <= 240) && (mouseY >= 300) && (mouseY <= 440)); }
boolean mousePressedLLB() { return ((mouseX >= 150) && (mouseX <= 190) && (mouseY >= 350) && (mouseY <= 490)); }

boolean mousePressedRLT() { return ((mouseX >= 350) && (mouseX <= 390) && (mouseY >= 250) && (mouseY <= 390)); }
boolean mousePressedRLL() { return ((mouseX >= 300) && (mouseX <= 340) && (mouseY >= 300) && (mouseY <= 440)); }
boolean mousePressedRLR() { return ((mouseX >= 400) && (mouseX <= 440) && (mouseY >= 300) && (mouseY <= 440)); }
boolean mousePressedRLB() { return ((mouseX >= 350) && (mouseX <= 390) && (mouseY >= 350) && (mouseY <= 490)); }

boolean mousePressedGAT() { return ((mouseX >= 550) && (mouseX <= 590) && (mouseY >= 250) && (mouseY <= 290)); }
boolean mousePressedGAL() { return ((mouseX >= 500) && (mouseX <= 540) && (mouseY >= 300) && (mouseY <= 340)); }
boolean mousePressedGAR() { return ((mouseX >= 600) && (mouseX <= 640) && (mouseY >= 300) && (mouseY <= 340)); }
boolean mousePressedGAB() { return ((mouseX >= 550) && (mouseX <= 590) && (mouseY >= 350) && (mouseY <= 390)); }

boolean mousePressedFreeMem() { return ((mouseX >= 270) && (mouseX <= 310) && (mouseY >= 715) && (mouseY <= 755)); }
