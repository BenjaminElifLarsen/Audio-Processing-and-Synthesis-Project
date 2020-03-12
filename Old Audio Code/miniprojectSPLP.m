classdef miniprojectSPLP < audioPlugin & matlab.System
    %UNTITLED2 Summary of this class goes here
    %   Detailed explanation goes here
    %audioTestBench
    
    properties
        
        SamplesPerFrame = 1024;
        SampleRate = 44100;
        
    end
    
    properties (Access = private)
        %SampleRate;
        
    end
    
    methods (Access = protected)
        
        function setupImpl(obj, u)
            global LpN;
            
            %% lp
            Lpfpass = 1000;
            Lpfstop = 2000;
            LpApass = 0.5;
            LpAstop = 10;
            %SamplesPerFrame=length(u);
            %SampleRate=getSampleRate(obj);
            fs = obj.SampleRate;
            f = fs/2;
            LpWPass = (2*pi*Lpfpass)/fs;
            LpWStop = (2*pi*Lpfstop)/fs;
            LPOmegaPass = tan(LpWPass/2);
            LpOmegaStop = tan(LpWStop/2);
            LpOmega = 2*pi*f;
            LpEpass = sqrt(10^(LpApass/10)-1);
            LpEstop = sqrt(10^(LpAstop/10)-1);
            Lpw = LpOmegaStop/LPOmegaPass;
            Lpe = LpEstop/LpEpass;
            LpNexact = log(Lpe)/log(Lpw);
            LpN = ceil(LpNexact);
            LpOmega0 = LPOmegaPass/(LpEpass^(1/LpN));
            Lpj = sqrt(-1);
            LpSc = Lpj*LpOmega; %S
            for i = 1:(2*LpN)
                Lptheta(i) = (pi/(2*LpN))*(LpN-1+(2*i));
                LpS(i) = LpOmega0*exp((Lpj*Lptheta(i)));
            end
            global LpG;
            global Lpa;
            
            if mod(LpN,2)==0
                k = LpN/2;
                for t = 1:k
                    LpthetaD(t+1) = -2*cos(Lptheta(t));
                end
            else
                k = LpN/2;
                LpthetaD(0+1) = (1+(LpOmega0));
                
                for t = 1:k
                    
                    LpthetaD(t+1) = -2*cos(Lptheta(t));
                    
                end
            end
            for i = 1:(LpN/2)
                LpG(i+1) = LpOmega0^2/(1-2*LpOmega0*cos(Lptheta(i))+LpOmega0^2);
                Lpa(i+1,1) = (2*(LpOmega0^2-1))/(1-2*LpOmega0*cos(Lptheta(i))+LpOmega0^2);
                Lpa(i+1,2) = (1+2*LpOmega0*cos(Lptheta(i))+LpOmega0^2)/(1-2*LpOmega0*cos(Lptheta(i))+LpOmega0^2);
               
                
            end
            
            if mod(LpN,2) == 0
                

            else
                LpG(0+1) = LpOmega0/(LpOmega0+1);
                Lpa(0+1,1) = (LpOmega0-1)/(LpOmega0+1);

                
            end
            
 

        end
        
        
        %%
        function ylp = stepImpl(obj, x)
            obj.SamplesPerFrame=length(x);
            obj.SampleRate=getSampleRate(obj);
            
            %%
            global LpN;
            global LpG;
            global Lpa;



            
            for ti = 1:LpN/2+1
                lpw(ti,1) = 0;
                lpw(ti,2) = 0;
                lpw(ti,3) = 0;
                lpw(ti,4) = 0;
                lpw(ti,5) = 0;
                
            end

            lpy = zeros(size(x));
            y = zeros(size(x));
            
            for t = 1:length(x)
                x1(1,:) = x(t,:);
                k = LpN/2;
                
                if mod(LpN,2) == 0 

                    lpy(1,:) = x1(1);
                    x1(1+1,:) = lpy(1,:);
                else
                    lpw(1,1) = x1(1)-Lpa(1,1)*lpw(1,1);
                    lpy(1,:) = LpG(1)*(lpw(1,1)+1);
                    x1(1+1,:) = lpy(1,:);
                end
                for i = 2:1:k+1
                    lpw(i,1) = x1(i)-Lpa(i,1)*lpw(i,2)-Lpa(i,2)*lpw(i,3);
                    lpy(i,:) = LpG(i)*lpw(i,1)+2*LpG(i)*lpw(i,2)+LpG(i)*lpw(i,3);
                    lpw(i,3) = lpw(i,2);
                    lpw(i,2) = lpw(i,1);
                    x1(i+1,:) = lpy(i,:);
                    
                end
                ylp(t,:) = lpy(floor(4),:);
                
            end
              ylp;
            
            

        end
end
end
    

