clc
clear
pause(0.01)
fig = figure;

%set(fig, 'buttondownfcn', 'mousebuttom(get(gcf, ''selectiontype''))');


%~isempty(findall(0,'Type','Figure'))
fs = 20000;
pos = 0.5;
mode1 = 0.12*sin(1.0*pos*pi);
mode2 = 0.02*sin(0.03+3.9*pos*pi);
mode3 = 0.03*sin(0.5+9.3*pos*pi);
mode = [mode1 mode2 mode3];
dec = [-0.0001 -0.0001 -0.0010];
vel = 1;
t = 1:fs;

pressure = 0;
z = 0;
counter = 1;
%global state;
%global selType;
%selType = [];
allowedSound = 0;
pitch_ = 0;
pause(2);
while ~isempty(findall(0,'Type','Figure'))
    %set(fig,'ButtonDownFcn', @Callback, 'KeyPressFcn', @KeyType_Callback);
    %f = gcf;
    %val = double(get(f,'CurrentCharacter'))
    %loc = get(0, 'PointerLocation'); %x,y
    siz = get(0, 'MonitorPositions'); %gets pixel size for multiple screens
    
    A = importdata('information.txt');
    
    if isempty(A)
        %A = importdata('information.txt');
        %%B = A{1};
        %allowedSound = 1;
    elseif iscell(A)
        B = A{1};
        if length(B) >= 31
            zAngle = str2num(B(1:6));
            theta = str2num(B(1+6:6+6));
            z = str2num(B(1+12:6+12));
            x = str2num(B(1+18:6+18));
            y = str2num(B(1+24:6+24));
            pressure = str2num(B(1+30:end));
            pressure = pressure/1023*10;
            y =  1036 - y;
            
            xnorm = x/1919;
            ynorm = y/1036;
            ynorm = ynorm*12;
            xnorm = xnorm*247;
            pitch_ = xnorm*ynorm;
            pitch_ = round(pitch_);
            zAngle = ((zAngle - 290) / (900 - 290))/2;
            if zAngle == 0
               zAngle = 0.0001;
            end
            theta = (theta - 0) / (3590 - 0);
            %czAngle(counter) = zAngle; %290-900
            %%cTheta(counter) = theta; %0 - 3590
            counter = counter+1;
        end
    end
    if z == 0 && allowedSound == 0 && pressure >= 1
        pos = zAngle;
        freq = pitch_;
        mode1 = 0.12*sin(1.0*pos*pi);
        mode2 = 0.02*sin(0.03+3.9*pos*pi);
        mode3 = 0.03*sin(0.5+9.3*pos*pi);
        mode = [mode1 mode2 mode3];
        %noise = rand(1,length(t));
        test = 1;
        index = 0;
        dec = [-0.0001*pressure -0.0001*pressure -0.0010*pressure];
        while test > 0.01
            index;
            test = exp(dec(1)*(index));
            index = index + 1;
        end
        t = 1:index;
        note = vel*(mode(1)*exp(dec(1)*t).*sin(freq*2*pi/fs*t)+mode(2)*exp(dec(2)*t).*sin(freq*2*2*pi/fs*t)+mode(3)*dec(3).*sin(freq*3*2*pi/fs*t));
        %note2 = note.*(noise*0.002.*(exp(dec(1)*t)));
        soundsc(note,fs)
        state = 0;
        allowedSound = 1;
        plot(note)
        str_ = num2str(pitch_);
        str_ = ['Frequency: ' str_];
        title(str_)
    elseif z >= 1
        allowedSound = 0;
    end
    
    %xlabel(xnorm);
    %ylabel(ynorm);
    %xpitch = xnorm*200+100;
    %ypitch = ynorm*200+100;
    %pitch = xpitch+ypitch;
    %angleSin = sin(90*0.0174532925);
    %angleCos = cos(90*0.0174532925);
    %angle = angleSin+angleCos;
    
    
    
    %selType = [];
    pause(1/fs);
    
end


% A = importdata('information.txt');
%
%
%
% B = A{1}
%
% B =
%
% 630   2720  1023  1546  459
%
% C = B(1:6)
%
% C =
%
% 630
%
% C = B(1:6)
%
% C =
%
% 630   2720


