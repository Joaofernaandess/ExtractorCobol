CBL TRUNC(BIN)
       IDENTIFICATION DIVISION.
       PROGRAM-ID. ICCP607B.
       ENVIRONMENT DIVISION.
       CONFIGURATION SECTION.
       SPECIAL-NAMES.
           DECIMAL-POINT IS COMMA.
       INPUT-OUTPUT SECTION.
       FILE-CONTROL.

           SELECT ICCF604I ASSIGN       TO     ICCF604I
              FILE STATUS  FS-ICCF604I.

           SELECT ICCL607O ASSIGN       TO     ICCL607O.
       
       DATA DIVISION.
       FILE SECTION.

       FD  ICCF604I
           RECORDING MODE IS F.
       01  REG-ICCF604I                 PIC  X(0200).

       FD  ICCL607O
           RECORDING MODE IS F.
       01  REG-ICCL607O                 PIC  X(1000).

       WORKING-STORAGE SECTION.

       01  WS-AUXILIARES.
           03 FS-ICCF604I               PIC  X(0002) VALUE '00'.
           03 WS-REGISTROS-ICCF604I     PIC  9(0009) VALUE 0.
           03 WS-REGISTROS-ICCL607O     PIC  9(0009) VALUE 0.  
           03 WS-REGISTROS-VAZIOS       PIC  9(0009) VALUE 0. 

           03 WS-COUNT-CTR               PIC S9(09) COMP-3 VALUE ZEROS.
           03 WS-COUNT-RENOV             PIC S9(09) COMP-3 VALUE ZEROS.
          
           03 WS-NUM-CTR-CSG            PIC  X(0020) VALUE SPACES.  

           03 WS-EXECUTA-CONSULTA-RENV  PIC  X(0001) VALUE SPACES. 
           03 WS-IDC-MMB                PIC  X(0001) VALUE SPACES. 
           03 WS-FLAG-NSU-LOG           PIC  X(0001) VALUE SPACES.     
           03 WS-CONTRATO               PIC  X(0012) VALUE SPACES.

           03 WS-COD-TIP-PPS            PIC  X(0001) VALUE SPACES. 

           03 WS-FLAG-WPP-MMB           PIC X(0001) VALUE SPACES.

           03 WS-IDT-PTB                PIC X(0001) VALUE SPACES.

           03 WS-SQLCODE                PIC  ---999.
           03 WS-TIMESTAMP32            PIC  X(0032) VALUE SPACES.
           
           03 WS-STRING-CNL-RLN.
              05 WS-VALOR-CNL-RLN       PIC  9(0004).
            
           03 WS-STRING-NSU-LOG.
              05 WS-VALOR-NSU-LOG       PIC  9(0009).
           03 WS-STRING-NSU-COF.
              05 WS-VALOR-NSU-COF       PIC  9(0009).
           03 WS-STRING-NUM-DND.
              05 WS-VALOR-NUM-DND       PIC  9(0004).
           03 WS-STRING-NUM-CTA.
              05 WS-VALOR-NUM-CTA       PIC  9(0009).
           03 WS-STRING-COD-PDT.
              05 WS-VALOR-COD-PDT       PIC  9(0004).
           03 WS-STRING-CTR-USU.
              05 WS-VALOR-CTR-USU       PIC  X(0012).
           03 WS-STRING-NUM-PES.
              05 WS-VALOR-NUM-PES       PIC  9(0009).

           03 WS-VLR-EPT-AUX.
              05 WS-VLR-EPT-AUX-S    PIC 9999999999999,99.

           03 WS-CTR-DATA-IMPLANT         PIC X(10) VALUE SPACES.   
           03 WS-CTR-DAT-CONTR            PIC X(10) VALUE SPACES.   

       01  WS-MODULOS.
           03 DBZS010S                    PIC  X(0008) VALUE 'DBZS010S'.

       01  WS-STRINGS.
           03 WS-DTA-MOV                PIC X(010).
           03 WS-VALOR-DTA-MOV          PIC X(010). 
           03 WS-NSU-LOG                PIC X(009).
           03 WS-NSU-COF                PIC X(009).
           03 WS-NUM-DND                PIC X(004).
           03 WS-NUM-CTA                PIC X(009).
           03 WS-COD-PDT                PIC x(004).
           03 WS-NUM-CTR-USU            PIC X(012).
           03 WS-NUM-PES                PIC X(009).
           03 WS-IDT-CNL-RLN            PIC X(004).
           03 WS-COD-CPF-CGC            PIC X(014).
           03 WS-IDC-PRE-DGT            PIC X(001).
           03 WS-IDC-REC-OPR            PIC X(001).

       01  WS-INDICADORES.
           03 WS-IND-IDC-MMB                PIC S9(04) COMP VALUE 0. 
           03 WS-IND-COD-TIP-PPS            PIC S9(04) COMP VALUE 0. 
           03 WS-IND-NUM-CTR-CSG            PIC S9(04) COMP VALUE 0.
           03 WS-IND-CTR-DAT-CONTR          PIC S9(04) COMP VALUE 0.
           03 IND-CTR-DATA-IMPLANT          PIC S9(04) COMP VALUE 0.
           03 IND-CONTRATO-SIC.
              05 IND-NUM-PES                PIC S9(04) COMP VALUE 0.
           03 IND-LOG-SIC-FEP-GAA.
              05 IND-COD-AGE-NGC            PIC S9(04) COMP VALUE 0.
              05 IND-COD-PDT                PIC S9(04) COMP VALUE 0.
              05 IND-DTA-CTB                PIC S9(04) COMP VALUE 0.
              05 IND-IDT-CNL-RLN            PIC S9(04) COMP VALUE 0.
              05 IND-NSU-LOG                PIC S9(04) COMP VALUE 0.
              05 IND-NSU-COF                PIC S9(04) COMP VALUE 0.
              05 IND-NUM-CTA                PIC S9(04) COMP VALUE 0.
              05 IND-NUM-CTA-BEN            PIC S9(04) COMP VALUE 0.
              05 IND-NUM-CTR-USU            PIC S9(04) COMP VALUE 0.
              05 IND-NUM-DND                PIC S9(04) COMP VALUE 0.
              05 IND-VLR-EPT                PIC S9(04) COMP VALUE 0.
              05 IND-DTA-MOV                PIC S9(04) COMP VALUE 0.
              05 IND-IDC-PRE-DGT            PIC S9(04) COMP VALUE 0.
              05 IND-IDC-REC-OPR            PIC S9(04) COMP VALUE 0.
              05 IND-DTA-STT-LOG            PIC S9(04) COMP VALUE 0.
              05 IND-COD-OGM                PIC S9(04) COMP VALUE 0.
              
           COPY DBZL010B.
           COPY ICCL604K.
           COPY ICCL607K.

           EXEC SQL INCLUDE DBZL001D END-EXEC.
           EXEC SQL INCLUDE SICL024D END-EXEC.
           EXEC SQL INCLUDE SICL184D END-EXEC.
           EXEC SQL INCLUDE IPCL019D END-EXEC.
           EXEC SQL INCLUDE SICL432D END-EXEC.

       PROCEDURE DIVISION.
       0100-PRINCIPAL.
           DISPLAY '--------------------------------------------'
           DISPLAY '* PROGRAMA ICCP607B INICIADO.'
           DISPLAY '--------------------------------------------'
           OPEN INPUT  ICCF604I.
           OPEN OUTPUT ICCL607O.
           EXEC SQL
              SET :WS-TIMESTAMP32 = CURRENT TIMESTAMP (12)
           END-EXEC.
           PERFORM 0400-PROCESSAR-ARQ-ICCF604I    THRU 0400-FIM.
           DISPLAY '==============================================='
           DISPLAY '*     FIM NORMAL DO PROGRAMA ICCP607B.         '
           DISPLAY '==============================================='
           DISPLAY '* QTD REGISTROS LIDOS NO ARQUIVO DE ENTRADA  = '
                      WS-REGISTROS-ICCF604I
           DISPLAY '* QTD TOTAL DE REG GRAVADOS NO ARQ DE SAIDA  = '
                      WS-REGISTROS-ICCL607O
           DISPLAY ' QTD REGISTRO DE CONTRATOS RENOVADOS         = ' 
                      WS-COUNT-RENOV  
           DISPLAY ' QTD DE REGISTROS VAZIOS NO ARQUIVO          = ' 
                      WS-REGISTROS-VAZIOS 
           DISPLAY '-----------------------------------------------'
           CLOSE ICCF604I.
           CLOSE ICCL607O.
           STOP RUN.

       0100-FIM. EXIT.

       0125-GRAVA-HEADER.
           MOVE SPACES      
             TO ICCL607K 
           MOVE IDT-EMP           OF ICCL604K 
             TO H-IDT-EMP         OF ICCL607K
           MOVE ';'
             TO  SEP-1            OF ICCL607K
                 SEP-2            OF ICCL607K
                 SEP-22           OF ICCL607K
           MOVE NUM-CTR-CSO-CRD   OF ICCL604K 
             TO H-NUM-CTR-CSO-CRD OF ICCL607K 
           MOVE TIPO-DOCUMENTO    OF ICCL604K 
             TO H-TIPO-DOCUMENTO  OF ICCL607K
           MOVE IDC-VLD           OF ICCL604K 
             TO H-IDC-VLD         OF ICCL607K  
           WRITE REG-ICCL607O FROM REG-HEADER-607.

       0125-FIM. EXIT.

       0200-LER-ARQUIVO-ICCF604I.
           READ ICCF604I INTO ICCL604K
           IF FS-ICCF604I NOT EQUAL '00' AND '10'
              DISPLAY '  ERRO NA LEITURA DO ARQUIVO - ICCF604I'
              DISPLAY '  FS-ICCF604I - ' FS-ICCF604I
              PERFORM 9999-ENCERRA-PROGRAMA      THRU 9999-FIM
           END-IF
           IF FS-ICCF604I EQUAL '00'
              COMPUTE WS-REGISTROS-ICCF604I = WS-REGISTROS-ICCF604I + 1
           END-IF.

       0200-FIM. EXIT.

       0400-PROCESSAR-ARQ-ICCF604I.
           PERFORM 0200-LER-ARQUIVO-ICCF604I       THRU 0200-FIM
           PERFORM 0125-GRAVA-HEADER               THRU 0125-FIM. 
           PERFORM 0200-LER-ARQUIVO-ICCF604I  THRU 0200-FIM    
           PERFORM WITH TEST BEFORE UNTIL NOT (FS-ICCF604I EQUAL '00')
              MOVE CTR-NUMERO-USUARIO OF ICCL604K
                TO WS-CONTRATO
              PERFORM 0800-CONSULTA-CONTRATOS THRU 0800-FIM
              IF SQL-OK 
                 PERFORM 0500-COMPOE-ARQUIVO-ICCL607O THRU 0500-FIM
                 IF WS-COD-TIP-PPS  EQUAL 'R' AND
                    WS-IDC-MMB      EQUAL 'N' AND 
                    WS-IDT-PTB      EQUAL 'N' AND 
                    NSU-LOG OF LOG-SIC-FEP-GAA EQUAL ZEROS
                    ADD 1 TO WS-COUNT-RENOV
                    PERFORM 0850-CONSULTA-CONTRATOS-RENV THRU 0850-FIM
                    IF SQL-OK 
                      PERFORM 0500-COMPOE-ARQUIVO-ICCL607O THRU 0500-FIM
                    END-IF
                  END-IF 
                  PERFORM 0550-GRAVA-ARQUIVO THRU 0550-FIM          
              END-IF
              MOVE SPACES TO  WS-NUM-CTR-CSG 
                              WS-IDC-MMB     
                              WS-CONTRATO    
                              WS-FLAG-WPP-MMB
                              WS-IDT-PTB  
                              WS-CTR-DAT-CONTR  
                              WS-COD-TIP-PPS 
              PERFORM 0200-LER-ARQUIVO-ICCF604I  THRU 0200-FIM
           END-PERFORM.

       0400-FIM. EXIT.

       0450-CONVERTE-PARA-STRING.
           MOVE IDT-CNL-RLN  OF LOG-SIC-FEP-GAA TO WS-VALOR-CNL-RLN
           STRING
                WS-STRING-CNL-RLN
                DELIMITED BY SIZE INTO WS-IDT-CNL-RLN
           END-STRING
           MOVE NSU-LOG OF LOG-SIC-FEP-GAA TO WS-VALOR-NSU-LOG
           STRING
                WS-STRING-NSU-LOG
                DELIMITED BY SIZE INTO WS-NSU-LOG
           END-STRING
           MOVE NSU-COF OF LOG-SIC-FEP-GAA TO WS-VALOR-NSU-COF
           STRING
                WS-STRING-NSU-COF
                DELIMITED BY SIZE INTO WS-NSU-COF
           END-STRING
           MOVE NUM-DND  OF LOG-SIC-FEP-GAA TO WS-VALOR-NUM-DND
           STRING
                WS-STRING-NUM-DND
                DELIMITED BY SIZE INTO WS-NUM-DND
           END-STRING
           MOVE NUM-CTA OF LOG-SIC-FEP-GAA TO WS-VALOR-NUM-CTA
           IF NUM-CTA OF LOG-SIC-FEP-GAA EQUAL ZEROS
              MOVE NUM-CTA-BEN OF LOG-SIC-FEP-GAA TO WS-VALOR-NUM-CTA
           END-IF
           STRING
                WS-STRING-NUM-CTA
                DELIMITED BY SIZE INTO WS-NUM-CTA
           END-STRING
           MOVE COD-PDT  OF LOG-SIC-FEP-GAA TO WS-VALOR-COD-PDT
           STRING
                WS-STRING-COD-PDT
                DELIMITED BY SIZE INTO WS-COD-PDT
           END-STRING
           MOVE NUM-CTR-USU  OF LOG-SIC-FEP-GAA TO WS-VALOR-CTR-USU
           STRING
                WS-STRING-CTR-USU
                DELIMITED BY SIZE INTO WS-NUM-CTR-USU
           END-STRING
           MOVE NUM-PES  OF CONTRATO-SIC TO WS-VALOR-NUM-PES
           STRING
                WS-STRING-NUM-PES
                DELIMITED BY SIZE INTO WS-NUM-PES
           END-STRING.
           PERFORM 0900-BUSCA-DADOS-PESSOA THRU 0900-FIM
           STRING
                COD-CPF-CGC  OF PESSOA
                DELIMITED BY SIZE INTO WS-COD-CPF-CGC
           END-STRING.

       0450-FIM. EXIT.

       0500-COMPOE-ARQUIVO-ICCL607O.
           PERFORM 0450-CONVERTE-PARA-STRING THRU 0450-FIM
           MOVE SPACES      
             TO ICCL607K
           MOVE ';'
             TO SEP-3  OF ICCL607K       
                SEP-4  OF ICCL607K
                SEP-5  OF ICCL607K
                SEP-6  OF ICCL607K
                SEP-7  OF ICCL607K
                SEP-8  OF ICCL607K
                SEP-9  OF ICCL607K
                SEP-10 OF ICCL607K
                SEP-11 OF ICCL607K
                SEP-12 OF ICCL607K
                SEP-13 OF ICCL607K
                SEP-14 OF ICCL607K
                SEP-15 OF ICCL607K
                SEP-16 OF ICCL607K
                SEP-17 OF ICCL607K
                SEP-18 OF ICCL607K
                SEP-19 OF ICCL607K
                SEP-20 OF ICCL607K
                SEP-21 OF ICCL607K
           MOVE WS-IDT-CNL-RLN
             TO IDT-CNL-RLN               OF ICCL607K.
           IF COD-OGM OF LOG-SIC-FEP-GAA EQUAL ZEROS
              MOVE SPACES 
                TO COD-OGM OF ICCL607K
              IF WS-IDT-PTB EQUAL 'S'
                 MOVE 0889 
                   TO COD-OGM OF ICCL607K
              END-IF
           ELSE
              MOVE COD-OGM OF LOG-SIC-FEP-GAA 
                TO COD-OGM OF ICCL607K
           END-IF   
           MOVE WS-CTR-DAT-CONTR
             TO DTA-MOV                   OF ICCL607K. 
           MOVE DTA-STT-LOG               OF LOG-SIC-FEP-GAA  
             TO DTA-STT-LOG               OF ICCL607K.
           MOVE WS-NSU-LOG
             TO NSU-LOG                   OF ICCL607K.
           MOVE WS-NSU-COF
             TO NSU-COF                   OF ICCL607K.
           MOVE WS-NUM-DND
             TO NUM-DND                   OF ICCL607K.
           MOVE WS-NUM-CTA
             TO NUM-CTA                   OF ICCL607K.
           MOVE WS-COD-PDT
             TO COD-PDT                   OF ICCL607K.
            MOVE VLR-EPT                  OF LOG-SIC-FEP-GAA
             TO WS-VLR-EPT-AUX-S.
           MOVE WS-VLR-EPT-AUX-S
             TO VLR-EPT                   OF ICCL607K.
           MOVE WS-NUM-CTR-USU
             TO NUM-CTR-USU               OF ICCL607K.
           MOVE WS-NUM-PES
             TO NUM-PES                   OF ICCL607K.
           MOVE WS-COD-CPF-CGC
             TO COD-CPF-CGC               OF ICCL607K.
           MOVE NOM-PES                   OF PESSOA
             TO NOM-PES                   OF ICCL607K.
           MOVE IDC-PRE-DGT               OF LOG-SIC-FEP-GAA
             TO IDC-PRE-DGT               OF ICCL607K.
           MOVE IDC-REC-OPR               OF LOG-SIC-FEP-GAA
             TO IDC-REC-OPR               OF ICCL607K.
           MOVE WS-IDC-MMB     
             TO IDC-MMB                   OF ICCL607K.
           MOVE WS-NUM-CTR-CSG  
             TO NUM-CTR-CSG               OF ICCL607K. 
           MOVE WS-IDT-PTB
             TO IDT-PTB                   OF ICCL607K.
           MOVE WS-CTR-DATA-IMPLANT  
             TO CTR-DATA-IMPLANT          OF ICCL607K.

       0500-FIM. EXIT.

       0550-GRAVA-ARQUIVO.
           WRITE REG-ICCL607O FROM REG-DETALHE-607
           COMPUTE WS-REGISTROS-ICCL607O = WS-REGISTROS-ICCL607O + 1.

       0550-FIM. EXIT.

       0560-GRAVA-REGISTRO-VAZIO.
           MOVE SPACES      
             TO ICCL607K
           MOVE ';'
             TO SEP-3  OF ICCL607K       
                SEP-4  OF ICCL607K
                SEP-5  OF ICCL607K
                SEP-6  OF ICCL607K
                SEP-7  OF ICCL607K
                SEP-8  OF ICCL607K
                SEP-9  OF ICCL607K
                SEP-10 OF ICCL607K
                SEP-11 OF ICCL607K
                SEP-12 OF ICCL607K
                SEP-13 OF ICCL607K
                SEP-14 OF ICCL607K
                SEP-15 OF ICCL607K
                SEP-16 OF ICCL607K
                SEP-17 OF ICCL607K
                SEP-18 OF ICCL607K
                SEP-19 OF ICCL607K
                SEP-20 OF ICCL607K
                SEP-21 OF ICCL607K
           MOVE NUM-CTR-USU  OF LOG-SIC-FEP-GAA 
             TO NUM-CTR-USU  OF ICCL607K    
           WRITE REG-ICCL607O FROM REG-DETALHE-607
           COMPUTE WS-REGISTROS-ICCL607O = WS-REGISTROS-ICCL607O + 1 
           COMPUTE WS-REGISTROS-VAZIOS   = WS-REGISTROS-VAZIOS + 1. 

       0560-FIM. EXIT.

       0800-CONSULTA-CONTRATOS.
           EXEC SQL 
               SELECT
                       CASE WHEN PC.IDT_PRC = 16 THEN 102
                            WHEN PC.IDT_PRC = 25 THEN 103
                            WHEN S.IDT_SLT_PTB IS NOT NULL THEN 84
                       ELSE CCS.IDT_CNL_RLN
                       END AS IDT_CNL_RLN,
                       LOG.COD_OGM,
                       LOG.DTA_STT_LOG,
                       LOG.COD_AGE_NGC,
                       LOG.DTA_CTB,
                       CASE WHEN S.IDT_SLT_PTB IS NOT NULL
                            THEN DATE(DTA_HOR_INC)
                            WHEN LOG.DTA_MOV IS NOT NULL 
                            THEN DTA_MOV
                            ELSE DATE(CONT.CTR_DAT_CONTR)
                            END AS DTA_MOV, 
                       DATE(CONT.CTR_DATA_IMPLANT),
                       CASE WHEN S.IDT_SLT_PTB IS NOT NULL
                            THEN s.NUM_SEQ_UNC
                            ELSE LOG.NSU_LOG END AS NSU_LOG,
                       LOG.NSU_COF,
                       LOG.NUM_DND,
                       LOG.NUM_CTA,
                       LOG.NUM_CTA_BEN,
                       CASE WHEN S.IDT_SLT_PTB IS NOT NULL
                            THEN S.COD_PDT
                            ELSE LOG.COD_PDT END AS COD_PDT,
                       CASE WHEN S.IDT_SLT_PTB IS NOT NULL
                            THEN S.SLD_DVD
                            ELSE LOG.VLR_EPT END AS VLR_EPT,
                       CONT.CTR_NUMERO_USUARIO,
                       CONT.NUM_PES,
                       LOG.IDC_PRE_DGT,
                       LOG.IDC_REC_OPR,
                       CASE WHEN PC.IDT_PRC IN (16, 25) THEN 'S'
                       ELSE 'N' END AS IDC_MMB,
                       CC.NUM_CTR_CSG,
                       CASE WHEN S.IDT_SLT_PTB IS NOT NULL THEN 'S'
                            ELSE 'N' END AS IDT_PTB,
                       CONT.COD_TIP_PPS     
                     INTO
                       :LOG-SIC-FEP-GAA.IDT-CNL-RLN
                          :IND-LOG-SIC-FEP-GAA.IND-IDT-CNL-RLN,
                       :LOG-SIC-FEP-GAA.COD-OGM
                          :IND-LOG-SIC-FEP-GAA.IND-COD-OGM,
                       :LOG-SIC-FEP-GAA.DTA-STT-LOG
                          :IND-LOG-SIC-FEP-GAA.IND-DTA-STT-LOG,
                       :LOG-SIC-FEP-GAA.COD-AGE-NGC
                          :IND-LOG-SIC-FEP-GAA.IND-COD-AGE-NGC,
                       :LOG-SIC-FEP-GAA.DTA-CTB
                          :IND-LOG-SIC-FEP-GAA.IND-DTA-CTB,
                       :WS-CTR-DAT-CONTR
                          :WS-IND-CTR-DAT-CONTR,
                       :WS-CTR-DATA-IMPLANT
                          :IND-CTR-DATA-IMPLANT,
                       :LOG-SIC-FEP-GAA.NSU-LOG
                          :IND-LOG-SIC-FEP-GAA.IND-NSU-LOG,
                       :LOG-SIC-FEP-GAA.NSU-COF
                          :IND-LOG-SIC-FEP-GAA.IND-NSU-COF,
                       :LOG-SIC-FEP-GAA.NUM-DND
                          :IND-LOG-SIC-FEP-GAA.IND-NUM-DND,
                       :LOG-SIC-FEP-GAA.NUM-CTA
                          :IND-LOG-SIC-FEP-GAA.IND-NUM-CTA,
                       :LOG-SIC-FEP-GAA.NUM-CTA-BEN
                          :IND-LOG-SIC-FEP-GAA.IND-NUM-CTA-BEN,
                       :LOG-SIC-FEP-GAA.COD-PDT
                          :IND-LOG-SIC-FEP-GAA.IND-COD-PDT,
                       :LOG-SIC-FEP-GAA.VLR-EPT
                          :IND-LOG-SIC-FEP-GAA.IND-VLR-EPT,
                       :LOG-SIC-FEP-GAA.NUM-CTR-USU
                          :IND-LOG-SIC-FEP-GAA.IND-NUM-CTR-USU,
                       :CONTRATO-SIC.NUM-PES
                          :IND-CONTRATO-SIC.IND-NUM-PES,
                       :LOG-SIC-FEP-GAA.IDC-PRE-DGT
                          :IND-LOG-SIC-FEP-GAA.IND-IDC-PRE-DGT,
                       :LOG-SIC-FEP-GAA.IDC-REC-OPR
                          :IND-LOG-SIC-FEP-GAA.IND-IDC-REC-OPR,
                       :WS-IDC-MMB
                          :WS-IND-IDC-MMB,
                       :WS-NUM-CTR-CSG
                          :WS-IND-NUM-CTR-CSG,
                       :WS-IDT-PTB, 
                       :WS-COD-TIP-PPS
                          :WS-IND-COD-TIP-PPS
                     FROM
                         CONTRATO_SIC CONT
                    LEFT JOIN COMPL_CONTRATO_SIC CCS
                       ON CONT.CTR_NUMERO = CCS.CTR_NUMERO
                    LEFT JOIN COMPL_CONTR_CONVEN CC
                       ON CONT.CTR_NUMERO = CC.CTR_NUMERO
                    LEFT JOIN CONTRATO_PRODUCAO PC
                       ON PC.CTR_NUMERO = CONT.CTR_NUMERO
                    LEFT JOIN LOG_SIC_FEP_GAA LOG
                       ON CONT.CTR_NUMERO_USUARIO = LOG.NUM_CTR_USU
                    LEFT JOIN CONTRATO_PORTADO CP
                         ON CP.CTR_NUMERO = CONT.CTR_NUMERO
                    LEFT JOIN CONTRAT_PORTAB_STC CS
                         ON CP.NUM_PTB = CS.NUM_PTB
                    LEFT JOIN SOLICITACAO_PORTAB S
                         ON CS.IDT_SLT_PTB = S.IDT_SLT_PTB
                         AND CONT.NUM_PES = S.NUM_PES
                   WHERE
                       CONT.CTR_NUMERO_USUARIO = :WS-CONTRATO
                 UNION ALL
                    SELECT
                       CASE WHEN PC.IDT_PRC = 16 THEN 102
                            WHEN PC.IDT_PRC = 25 THEN 103
                            WHEN S.IDT_SLT_PTB IS NOT NULL THEN 84
                       ELSE CCS.IDT_CNL_RLN
                       END AS IDT_CNL_RLN,
                       LOG.COD_OGM,
                       LOG.DTA_STT_LOG,
                       LOG.COD_AGE_NGC,
                       LOG.DTA_CTB,
                       CASE WHEN S.IDT_SLT_PTB IS NOT NULL
                            THEN DATE(DTA_HOR_INC)
                            WHEN LOG.DTA_MOV IS NOT NULL 
                            THEN DTA_MOV
                            ELSE DATE(CONT.CTR_DAT_CONTR)
                            END AS DTA_MOV, 
                       DATE(CONT.CTR_DATA_IMPLANT),
                       CASE WHEN S.IDT_SLT_PTB IS NOT NULL
                            THEN s.NUM_SEQ_UNC
                            ELSE LOG.NSU_LOG END AS NSU_LOG,
                       LOG.NSU_COF,
                       LOG.NUM_DND,
                       LOG.NUM_CTA,
                       LOG.NUM_CTA_BEN,
                       CASE WHEN S.IDT_SLT_PTB IS NOT NULL
                            THEN S.COD_PDT
                            ELSE LOG.COD_PDT END AS COD_PDT,
                       CASE WHEN S.IDT_SLT_PTB IS NOT NULL
                            THEN S.SLD_DVD
                            ELSE LOG.VLR_EPT END AS VLR_EPT,
                       CONT.CTR_NUMERO_USUARIO,
                       CONT.NUM_PES,
                       LOG.IDC_PRE_DGT,
                       LOG.IDC_REC_OPR,
                       CASE WHEN PC.IDT_PRC IN (16, 25) THEN 'S'
                       ELSE 'N' END AS IDC_MMB,
                       CC.NUM_CTR_CSG,
                       CASE WHEN S.IDT_SLT_PTB IS NOT NULL THEN 'S'
                            ELSE 'N' END AS IDT_PTB,
                       CONT.COD_TIP_PPS      
                     INTO
                       :LOG-SIC-FEP-GAA.IDT-CNL-RLN
                          :IND-LOG-SIC-FEP-GAA.IND-IDT-CNL-RLN,
                       :LOG-SIC-FEP-GAA.COD-OGM
                          :IND-LOG-SIC-FEP-GAA.IND-COD-OGM,
                       :LOG-SIC-FEP-GAA.DTA-STT-LOG
                          :IND-LOG-SIC-FEP-GAA.IND-DTA-STT-LOG,
                       :LOG-SIC-FEP-GAA.COD-AGE-NGC
                          :IND-LOG-SIC-FEP-GAA.IND-COD-AGE-NGC,
                       :LOG-SIC-FEP-GAA.DTA-CTB
                          :IND-LOG-SIC-FEP-GAA.IND-DTA-CTB,
                       :WS-CTR-DAT-CONTR
                          :WS-IND-CTR-DAT-CONTR,
                       :WS-CTR-DATA-IMPLANT
                          :IND-CTR-DATA-IMPLANT,
                       :LOG-SIC-FEP-GAA.NSU-LOG
                          :IND-LOG-SIC-FEP-GAA.IND-NSU-LOG,
                       :LOG-SIC-FEP-GAA.NSU-COF
                          :IND-LOG-SIC-FEP-GAA.IND-NSU-COF,
                       :LOG-SIC-FEP-GAA.NUM-DND
                          :IND-LOG-SIC-FEP-GAA.IND-NUM-DND,
                       :LOG-SIC-FEP-GAA.NUM-CTA
                          :IND-LOG-SIC-FEP-GAA.IND-NUM-CTA,
                       :LOG-SIC-FEP-GAA.NUM-CTA-BEN
                          :IND-LOG-SIC-FEP-GAA.IND-NUM-CTA-BEN,
                       :LOG-SIC-FEP-GAA.COD-PDT
                          :IND-LOG-SIC-FEP-GAA.IND-COD-PDT,
                       :LOG-SIC-FEP-GAA.VLR-EPT
                          :IND-LOG-SIC-FEP-GAA.IND-VLR-EPT,
                       :LOG-SIC-FEP-GAA.NUM-CTR-USU
                          :IND-LOG-SIC-FEP-GAA.IND-NUM-CTR-USU,
                       :CONTRATO-SIC.NUM-PES
                          :IND-CONTRATO-SIC.IND-NUM-PES,
                       :LOG-SIC-FEP-GAA.IDC-PRE-DGT
                          :IND-LOG-SIC-FEP-GAA.IND-IDC-PRE-DGT,
                       :LOG-SIC-FEP-GAA.IDC-REC-OPR
                          :IND-LOG-SIC-FEP-GAA.IND-IDC-REC-OPR,
                       :WS-IDC-MMB
                          :WS-IND-IDC-MMB,
                       :WS-NUM-CTR-CSG
                          :WS-IND-NUM-CTR-CSG,
                       :WS-IDT-PTB,
                       :WS-COD-TIP-PPS
                          :WS-IND-COD-TIP-PPS
                     FROM
                         HST_CONTRATO_SIC CONT
                    LEFT JOIN HST_COMPL_CTR_SIC CCS
                       ON CONT.CTR_NUMERO = CCS.CTR_NUMERO
                    LEFT JOIN HST_COMPL_CTR_CONV CC
                       ON CONT.CTR_NUMERO = CC.CTR_NUMERO
                    LEFT JOIN HST_CONTR_PRODUCAO PC
                       ON PC.CTR_NUMERO = CONT.CTR_NUMERO
                    LEFT JOIN LOG_SIC_FEP_GAA LOG
                       ON CONT.CTR_NUMERO_USUARIO = LOG.NUM_CTR_USU
                    LEFT JOIN HST_CONTRAT_PORTAD CP
                         ON CP.CTR_NUMERO = CONT.CTR_NUMERO
                    LEFT JOIN CONTRAT_PORTAB_STC  CS
                         ON CP.NUM_PTB = CS.NUM_PTB
                    LEFT JOIN SOLICITACAO_PORTAB S
                         ON CS.IDT_SLT_PTB = S.IDT_SLT_PTB
                         AND CONT.NUM_PES = S.NUM_PES
                   WHERE
                       CONT.CTR_NUMERO_USUARIO = :WS-CONTRATO
           END-EXEC

           IF NOT SQL-OK AND NOT SQL-ROW-NOT-FOUND
              MOVE 'S' TO L-ACAO
              MOVE 'E' TO L-FUNCAO
              MOVE SQLCODE TO WS-SQLCODE
              DISPLAY '*-----------------------------------------*'
              DISPLAY '* ERRO CONSULTA - 0800-CONSULTA-CONTRATOS'
              DISPLAY '* SQLCODE     = ' WS-SQLCODE
              DISPLAY '* WS-CONTRATO = ' WS-CONTRATO
              DISPLAY '* NUMERO DA LINHA DO ARQUIVO: ' 
                         WS-REGISTROS-ICCF604I
              DISPLAY '*-----------------------------------------*'
              PERFORM 8888-ROTINA-MSG-DB2         THRU 8888-FIM
              PERFORM 9999-ENCERRA-PROGRAMA       THRU 9999-FIM
           END-IF.

           IF SQL-ROW-NOT-FOUND
              DISPLAY '*-----------------------------------------*'
              DISPLAY '* CONTRATO NAO ENCONTRADO'
              DISPLAY '* WS-CONTRATO = ' WS-CONTRATO
              DISPLAY '* NUMERO DA LINHA DO ARQUIVO: ' 
                         WS-REGISTROS-ICCF604I
              DISPLAY '*-----------------------------------------*'
              PERFORM 0560-GRAVA-REGISTRO-VAZIO THRU 0560-FIM
           END-IF.

           IF SQL-OK
              IF WS-IND-COD-TIP-PPS < 0
                 MOVE SPACES TO WS-COD-TIP-PPS
              END-IF  
              IF IND-COD-OGM OF IND-LOG-SIC-FEP-GAA < 0
                 MOVE ZEROS TO COD-OGM OF LOG-SIC-FEP-GAA
              END-IF
              IF IND-IDT-CNL-RLN OF IND-LOG-SIC-FEP-GAA < 0
                 MOVE ZEROS TO IDT-CNL-RLN OF LOG-SIC-FEP-GAA
              END-IF
              IF IND-COD-AGE-NGC OF IND-LOG-SIC-FEP-GAA < 0
                 MOVE ZEROS TO COD-AGE-NGC OF LOG-SIC-FEP-GAA
              END-IF
              IF IND-DTA-CTB OF IND-LOG-SIC-FEP-GAA < 0
                 MOVE SPACES TO DTA-CTB OF LOG-SIC-FEP-GAA
              END-IF
              IF IND-NSU-LOG OF IND-LOG-SIC-FEP-GAA < 0
                 MOVE ZEROS TO NSU-LOG OF LOG-SIC-FEP-GAA
              END-IF
              IF IND-NSU-COF OF IND-LOG-SIC-FEP-GAA < 0
                 MOVE ZEROS TO NSU-COF OF LOG-SIC-FEP-GAA
              END-IF
              IF IND-NUM-DND OF IND-LOG-SIC-FEP-GAA < 0
                 MOVE ZEROS TO NUM-DND OF LOG-SIC-FEP-GAA
              END-IF
              IF IND-NUM-CTA OF IND-LOG-SIC-FEP-GAA < 0
                 MOVE ZEROS TO NUM-CTA OF LOG-SIC-FEP-GAA
              END-IF
              IF IND-NUM-CTA-BEN OF IND-LOG-SIC-FEP-GAA < 0
                 MOVE ZEROS TO NUM-CTA-BEN OF LOG-SIC-FEP-GAA
              END-IF
              IF IND-COD-PDT OF IND-LOG-SIC-FEP-GAA < 0
                 MOVE ZEROS TO COD-PDT OF LOG-SIC-FEP-GAA
              END-IF
              IF IND-VLR-EPT OF IND-LOG-SIC-FEP-GAA < 0
                 MOVE ZEROS TO VLR-EPT OF LOG-SIC-FEP-GAA
              END-IF
              IF IND-NUM-CTR-USU OF IND-LOG-SIC-FEP-GAA < 0
                 MOVE SPACES TO NUM-CTR-USU OF LOG-SIC-FEP-GAA
              END-IF
              IF IND-NUM-PES OF IND-CONTRATO-SIC < 0
                 MOVE ZEROS TO NUM-PES OF CONTRATO-SIC
              END-IF
              IF IND-DTA-MOV OF IND-LOG-SIC-FEP-GAA  < 0
                 MOVE SPACES TO DTA-MOV OF LOG-SIC-FEP-GAA
              END-IF 
              IF IND-DTA-STT-LOG  OF IND-LOG-SIC-FEP-GAA  < 0
                 MOVE SPACES TO DTA-STT-LOG OF LOG-SIC-FEP-GAA
              END-IF
              IF IND-IDC-PRE-DGT OF IND-LOG-SIC-FEP-GAA  < 0
                 MOVE SPACES TO IDC-PRE-DGT OF LOG-SIC-FEP-GAA
              END-IF
              IF IND-IDC-REC-OPR OF IND-LOG-SIC-FEP-GAA  < 0
                 MOVE SPACES TO IDC-REC-OPR OF LOG-SIC-FEP-GAA
              END-IF
              IF WS-IND-IDC-MMB  < 0
                 MOVE SPACES TO WS-IDC-MMB  
              END-IF
              IF WS-IND-NUM-CTR-CSG  < 0
                 MOVE SPACES TO WS-NUM-CTR-CSG 
              END-IF   
           END-IF.
           
       0800-FIM. EXIT.

       0850-CONSULTA-CONTRATOS-RENV.
           EXEC SQL
               SELECT 
                       CASE WHEN PC.IDT_PRC = 16 THEN 102
                            WHEN PC.IDT_PRC = 25 THEN 103
                            WHEN S.IDT_SLT_PTB IS NOT NULL THEN 84
                       ELSE CCS.IDT_CNL_RLN
                       END AS IDT_CNL_RLN ,
                       0889,
                       LOG.DTA_STT_LOG,
                       LOG.COD_AGE_NGC,
                       LOG.DTA_CTB,
                       CASE WHEN S.IDT_SLT_PTB IS NOT NULL
                            THEN DATE(DTA_HOR_INC)
                            WHEn log.dta_mov IS NOT NULL  
                            THEN log.dta_mov
                            ELSE DATE(CONT.CTR_DAT_CONTR)
                            END AS DTA_MOV,
                       DATE(CONT.CTR_DATA_IMPLANT),
                       CASE WHEN S.IDT_SLT_PTB IS NOT NULL
                            THEN s.NUM_SEQ_UNC
                            ELSE LOG.NSU_LOG END AS NSU_LOG,
                       LOG.NSU_COF,
                       LOG.NUM_DND,
                       LOG.NUM_CTA,
                       LOG.NUM_CTA_BEN,
                       CASE WHEN S.IDT_SLT_PTB IS NOT NULL
                            THEN S.COD_PDT
                            ELSE LOG.COD_PDT END AS COD_PDT,
                       CASE WHEN S.IDT_SLT_PTB IS NOT NULL
                            THEN S.SLD_DVD
                            ELSE LOG.VLR_EPT END AS VLR_EPT,
                       CONT.CTR_NUMERO_USUARIO,
                       CONT.NUM_PES,
                       LOG.IDC_PRE_DGT,
                       LOG.IDC_REC_OPR,
                       CASE WHEN PC.IDT_PRC IN (16, 25) THEN 'S'
                       ELSE 'N' END AS IDC_MMB,
                       CC.NUM_CTR_CSG, 
                       CASE WHEN S.IDT_SLT_PTB IS NOT NULL THEN 'S'
                            ELSE 'N' END AS IDT_PTB, 
                       CONT.COD_TIP_PPS 
                     INTO
                       :LOG-SIC-FEP-GAA.IDT-CNL-RLN
                          :IND-LOG-SIC-FEP-GAA.IND-IDT-CNL-RLN,
                       :LOG-SIC-FEP-GAA.COD-OGM
                          :IND-LOG-SIC-FEP-GAA.IND-COD-OGM,
                       :LOG-SIC-FEP-GAA.DTA-STT-LOG
                          :IND-LOG-SIC-FEP-GAA.IND-DTA-STT-LOG,
                       :LOG-SIC-FEP-GAA.COD-AGE-NGC
                          :IND-LOG-SIC-FEP-GAA.IND-COD-AGE-NGC,
                       :LOG-SIC-FEP-GAA.DTA-CTB
                          :IND-LOG-SIC-FEP-GAA.IND-DTA-CTB,
                       :WS-CTR-DAT-CONTR
                          :WS-IND-CTR-DAT-CONTR,
                       :WS-CTR-DATA-IMPLANT
                          :IND-CTR-DATA-IMPLANT,
                       :LOG-SIC-FEP-GAA.NSU-LOG
                          :IND-LOG-SIC-FEP-GAA.IND-NSU-LOG,
                       :LOG-SIC-FEP-GAA.NSU-COF
                          :IND-LOG-SIC-FEP-GAA.IND-NSU-COF,
                       :LOG-SIC-FEP-GAA.NUM-DND
                          :IND-LOG-SIC-FEP-GAA.IND-NUM-DND,
                       :LOG-SIC-FEP-GAA.NUM-CTA
                          :IND-LOG-SIC-FEP-GAA.IND-NUM-CTA,
                       :LOG-SIC-FEP-GAA.NUM-CTA-BEN
                          :IND-LOG-SIC-FEP-GAA.IND-NUM-CTA-BEN,
                       :LOG-SIC-FEP-GAA.COD-PDT
                          :IND-LOG-SIC-FEP-GAA.IND-COD-PDT,
                       :LOG-SIC-FEP-GAA.VLR-EPT
                          :IND-LOG-SIC-FEP-GAA.IND-VLR-EPT,
                       :LOG-SIC-FEP-GAA.NUM-CTR-USU
                          :IND-LOG-SIC-FEP-GAA.IND-NUM-CTR-USU,
                       :CONTRATO-SIC.NUM-PES
                          :IND-CONTRATO-SIC.IND-NUM-PES,
                       :LOG-SIC-FEP-GAA.IDC-PRE-DGT
                          :IND-LOG-SIC-FEP-GAA.IND-IDC-PRE-DGT,
                       :LOG-SIC-FEP-GAA.IDC-REC-OPR
                          :IND-LOG-SIC-FEP-GAA.IND-IDC-REC-OPR,
                       :WS-IDC-MMB
                          :WS-IND-IDC-MMB,
                       :WS-NUM-CTR-CSG
                          :WS-IND-NUM-CTR-CSG,
                       :WS-IDT-PTB,  
                       :WS-COD-TIP-PPS
                          :WS-IND-COD-TIP-PPS     
                     FROM
                          CONTRATO_SIC CONT
                    JOIN  COMPL_CONTRATO_SIC CCS
                       ON CONT.CTR_NUMERO = CCS.CTR_NUMERO
                    LEFT JOIN  COMPL_CONTR_CONVEN CC
                       ON CONT.CTR_NUMERO = CC.CTR_NUMERO
                    LEFT JOIN  CONTRATO_PRODUCAO PC
                       ON PC.CTR_NUMERO = CONT.CTR_NUMERO
                    LEFT JOIN LOG_SIC_FEP_GAA LOG
                       ON CONT.CTR_NUMERO_USUARIO = LOG.NUM_CTR_USU
                    JOIN   SOLICITACAO_PORTAB S
                       ON CONT.CTR_NUMERO_USUARIO = S.IDT_CTR_USU_RNV 
                       AND S.IDT_STT_PTB IN (26, 28)
                       AND CONT.NUM_PES = S.NUM_PES
                   WHERE 
                       CONT.CTR_NUMERO_USUARIO = :WS-CONTRATO
                       AND cont.COD_TIP_PPS = 'R'
               UNION ALL
                    SELECT 
                       CASE WHEN PC.IDT_PRC = 16 THEN 102
                            WHEN PC.IDT_PRC = 25 THEN 103
                            WHEN S.IDT_SLT_PTB IS NOT NULL THEN 84
                       ELSE CCS.IDT_CNL_RLN
                       END AS IDT_CNL_RLN ,
                       0889,
                       LOG.DTA_STT_LOG,
                       LOG.COD_AGE_NGC,
                       LOG.DTA_CTB,
                       CASE WHEN S.IDT_SLT_PTB IS NOT NULL
                            THEN DATE(DTA_HOR_INC)
                            WHEn log.dta_mov IS NOT NULL  
                            THEN log.dta_mov
                            ELSE DATE(CONT.CTR_DAT_CONTR)
                            END AS CTR_DAT_CONTR,
                       DATE(CONT.CTR_DATA_IMPLANT),
                         CASE WHEN S.IDT_SLT_PTB IS NOT NULL
                            THEN s.NUM_SEQ_UNC
                            ELSE LOG.NSU_LOG END AS NSU_LOG,
                       LOG.NSU_COF,
                       LOG.NUM_DND,
                       LOG.NUM_CTA,
                       LOG.NUM_CTA_BEN,
                       CASE WHEN S.IDT_SLT_PTB IS NOT NULL
                            THEN S.COD_PDT
                            ELSE LOG.COD_PDT END AS COD_PDT,
                       CASE WHEN S.IDT_SLT_PTB IS NOT NULL
                            THEN S.SLD_DVD
                            ELSE LOG.VLR_EPT END AS VLR_EPT,
                       CONT.CTR_NUMERO_USUARIO,
                       CONT.NUM_PES,
                       LOG.IDC_PRE_DGT,
                       LOG.IDC_REC_OPR,
                       CASE WHEN PC.IDT_PRC IN (16, 25) THEN 'S'
                       ELSE 'N' END AS IDC_MMB,
                       CC.NUM_CTR_CSG, 
                       CASE WHEN S.IDT_SLT_PTB IS NOT NULL THEN 'S'
                            ELSE 'N' END AS IDT_PTB , 
                       CONT.COD_TIP_PPS 
                     INTO
                       :LOG-SIC-FEP-GAA.IDT-CNL-RLN
                          :IND-LOG-SIC-FEP-GAA.IND-IDT-CNL-RLN,
                       :LOG-SIC-FEP-GAA.COD-OGM
                          :IND-LOG-SIC-FEP-GAA.IND-COD-OGM,
                       :LOG-SIC-FEP-GAA.DTA-STT-LOG
                          :IND-LOG-SIC-FEP-GAA.IND-DTA-STT-LOG,
                       :LOG-SIC-FEP-GAA.COD-AGE-NGC
                          :IND-LOG-SIC-FEP-GAA.IND-COD-AGE-NGC,
                       :LOG-SIC-FEP-GAA.DTA-CTB
                          :IND-LOG-SIC-FEP-GAA.IND-DTA-CTB,
                       :WS-CTR-DAT-CONTR
                          :WS-IND-CTR-DAT-CONTR,
                       :WS-CTR-DATA-IMPLANT
                          :IND-CTR-DATA-IMPLANT,
                       :LOG-SIC-FEP-GAA.NSU-LOG
                          :IND-LOG-SIC-FEP-GAA.IND-NSU-LOG,
                       :LOG-SIC-FEP-GAA.NSU-COF
                          :IND-LOG-SIC-FEP-GAA.IND-NSU-COF,
                       :LOG-SIC-FEP-GAA.NUM-DND
                          :IND-LOG-SIC-FEP-GAA.IND-NUM-DND,
                       :LOG-SIC-FEP-GAA.NUM-CTA
                          :IND-LOG-SIC-FEP-GAA.IND-NUM-CTA,
                       :LOG-SIC-FEP-GAA.NUM-CTA-BEN
                          :IND-LOG-SIC-FEP-GAA.IND-NUM-CTA-BEN,
                       :LOG-SIC-FEP-GAA.COD-PDT
                          :IND-LOG-SIC-FEP-GAA.IND-COD-PDT,
                       :LOG-SIC-FEP-GAA.VLR-EPT
                          :IND-LOG-SIC-FEP-GAA.IND-VLR-EPT,
                       :LOG-SIC-FEP-GAA.NUM-CTR-USU
                          :IND-LOG-SIC-FEP-GAA.IND-NUM-CTR-USU,
                       :CONTRATO-SIC.NUM-PES
                          :IND-CONTRATO-SIC.IND-NUM-PES,
                       :LOG-SIC-FEP-GAA.IDC-PRE-DGT
                          :IND-LOG-SIC-FEP-GAA.IND-IDC-PRE-DGT,
                       :LOG-SIC-FEP-GAA.IDC-REC-OPR
                          :IND-LOG-SIC-FEP-GAA.IND-IDC-REC-OPR,
                       :WS-IDC-MMB
                          :WS-IND-IDC-MMB,
                       :WS-NUM-CTR-CSG
                          :WS-IND-NUM-CTR-CSG,
                       :WS-IDT-PTB,
                       :WS-COD-TIP-PPS
                          :WS-IND-COD-TIP-PPS
                          
                     FROM
                          HST_CONTRATO_SIC CONT
                    JOIN  HST_COMPL_CTR_SIC CCS
                       ON CONT.CTR_NUMERO = CCS.CTR_NUMERO
                    LEFT JOIN  HST_COMPL_CTR_CONV CC
                       ON CONT.CTR_NUMERO = CC.CTR_NUMERO
                    LEFT JOIN  HST_CONTR_PRODUCAO PC
                       ON PC.CTR_NUMERO = CONT.CTR_NUMERO
                    LEFT JOIN LOG_SIC_FEP_GAA LOG
                       ON CONT.CTR_NUMERO_USUARIO = LOG.NUM_CTR_USU
                    JOIN  SOLICITACAO_PORTAB S
                         ON CONT.CTR_NUMERO_USUARIO = S.IDT_CTR_USU_RNV 
                         AND S.IDT_STT_PTB IN (26,28)
                         AND CONT.NUM_PES = S.NUM_PES
                   WHERE
                       CONT.CTR_NUMERO_USUARIO = :WS-CONTRATO

           END-EXEC.

           IF NOT SQL-OK AND NOT SQL-ROW-NOT-FOUND
              MOVE 'S' TO L-ACAO
              MOVE 'E' TO L-FUNCAO
              MOVE SQLCODE TO WS-SQLCODE
              DISPLAY '*-------------------------------------------*'
              DISPLAY '* ERRO CONSULTA - 0850-CONSULTA-CONTRATOS-RENV'
              DISPLAY '* SQLCODE     = ' WS-SQLCODE
              DISPLAY '* WS-CONTRATO = ' WS-CONTRATO
              DISPLAY '* NUMERO DA LINHA DO ARQUIVO: ' 
                         WS-REGISTROS-ICCF604I
              DISPLAY '*-------------------------------------------*'
              PERFORM 8888-ROTINA-MSG-DB2         THRU 8888-FIM
              PERFORM 9999-ENCERRA-PROGRAMA       THRU 9999-FIM
           END-IF.

           IF SQL-OK
              IF WS-IND-COD-TIP-PPS < 0
                 MOVE SPACES TO WS-COD-TIP-PPS
              END-IF 
              IF IND-COD-OGM OF IND-LOG-SIC-FEP-GAA < 0
                 MOVE ZEROS TO COD-OGM OF LOG-SIC-FEP-GAA
              END-IF            
              IF IND-IDT-CNL-RLN OF IND-LOG-SIC-FEP-GAA < 0
                 MOVE ZEROS TO IDT-CNL-RLN OF LOG-SIC-FEP-GAA
              END-IF
              IF IND-COD-AGE-NGC OF IND-LOG-SIC-FEP-GAA < 0
                 MOVE ZEROS TO COD-AGE-NGC OF LOG-SIC-FEP-GAA
              END-IF
              IF IND-DTA-CTB OF IND-LOG-SIC-FEP-GAA < 0
                 MOVE SPACES TO DTA-CTB OF LOG-SIC-FEP-GAA
              END-IF
              IF IND-NSU-LOG OF IND-LOG-SIC-FEP-GAA < 0
                 MOVE ZEROS TO NSU-LOG OF LOG-SIC-FEP-GAA
              END-IF
              IF IND-NSU-COF OF IND-LOG-SIC-FEP-GAA < 0
                 MOVE ZEROS TO NSU-COF OF LOG-SIC-FEP-GAA
              END-IF
              IF IND-NUM-DND OF IND-LOG-SIC-FEP-GAA < 0
                 MOVE ZEROS TO NUM-DND OF LOG-SIC-FEP-GAA
              END-IF
              IF IND-NUM-CTA OF IND-LOG-SIC-FEP-GAA < 0
                 MOVE ZEROS TO NUM-CTA OF LOG-SIC-FEP-GAA
              END-IF
              IF IND-NUM-CTA-BEN OF IND-LOG-SIC-FEP-GAA < 0
                 MOVE ZEROS TO NUM-CTA-BEN OF LOG-SIC-FEP-GAA
              END-IF
              IF IND-COD-PDT OF IND-LOG-SIC-FEP-GAA < 0
                 MOVE ZEROS TO COD-PDT OF LOG-SIC-FEP-GAA
              END-IF
              IF IND-VLR-EPT OF IND-LOG-SIC-FEP-GAA < 0
                 MOVE ZEROS TO VLR-EPT OF LOG-SIC-FEP-GAA
              END-IF
              IF IND-NUM-CTR-USU OF IND-LOG-SIC-FEP-GAA < 0
                 MOVE SPACES TO NUM-CTR-USU OF LOG-SIC-FEP-GAA
              END-IF
              IF IND-NUM-PES OF IND-CONTRATO-SIC < 0
                 MOVE ZEROS TO NUM-PES OF CONTRATO-SIC
              END-IF
              IF IND-DTA-MOV OF IND-LOG-SIC-FEP-GAA  < 0
                 MOVE SPACES TO DTA-MOV OF LOG-SIC-FEP-GAA
              END-IF 
              IF IND-DTA-STT-LOG  OF IND-LOG-SIC-FEP-GAA  < 0
                 MOVE SPACES TO DTA-STT-LOG OF LOG-SIC-FEP-GAA
              END-IF
              IF IND-IDC-PRE-DGT OF IND-LOG-SIC-FEP-GAA  < 0
                 MOVE SPACES TO IDC-PRE-DGT OF LOG-SIC-FEP-GAA
              END-IF
              IF IND-IDC-REC-OPR OF IND-LOG-SIC-FEP-GAA  < 0
                 MOVE SPACES TO IDC-REC-OPR OF LOG-SIC-FEP-GAA
              END-IF
              IF WS-IND-IDC-MMB  < 0
                 MOVE SPACES TO WS-IDC-MMB  
              END-IF
              IF WS-IND-NUM-CTR-CSG  < 0
                 MOVE SPACES TO WS-NUM-CTR-CSG 
              END-IF   
           END-IF.

       0850-FIM. EXIT.

       0900-BUSCA-DADOS-PESSOA.
           MOVE NUM-PES OF CONTRATO-SIC
             TO NUM-PES OF PESSOA
           EXEC SQL
              SELECT
                   NOM_PES,
                   COD_CPF_CGC
                INTO
                   :PESSOA.NOM-PES,
                   :PESSOA.COD-CPF-CGC
                FROM
                   PESSOA
               WHERE
                   NUM_PES = :PESSOA.NUM-PES 
           END-EXEC.

           IF NOT SQL-OK
              DISPLAY '**************************************'
              DISPLAY '*      ERRO NA CONSULTA PESSOA       *'
              DISPLAY ' NUM_PES            = ' NUM-PES OF PESSOA
              DISPLAY ' CTR_NUMERO_USUARIO = ' WS-CONTRATO
              DISPLAY '**************************************'
              MOVE 'S' TO L-ACAO
              MOVE 'E' TO L-FUNCAO
              PERFORM 8888-ROTINA-MSG-DB2    THRU 8888-FIM
              PERFORM 9999-ENCERRA-PROGRAMA  THRU 9999-FIM
           END-IF.

       0900-FIM. EXIT.

       0950-CONSULTA-COMPLETA.
           EXEC SQL
               SELECT
                   CASE WHEN PC.IDT_PRC = 16 THEN 102
                        WHEN PC.IDT_PRC = 25 THEN 103
                        WHEN S.IDT_SLT_PTB IS NOT NULL THEN 84
                   ELSE CCS.IDT_CNL_RLN
                   END AS IDT_CNL_RLN,
                   LOG.COD_OGM,
                   LOG.DTA_MOV,
                   LOG.DTA_STT_LOG,
                   CASE WHEN S.IDT_SLT_PTB IS NOT NULL
                        THEN S.NUM_SEQ_UNC
                        ELSE LOG.NSU_LOG END AS NSU_LOG,
                   LOG.NSU_COF,
                   LOG.NUM_DND,
                   LOG.NUM_CTA,
                   CASE WHEN S.IDT_SLT_PTB IS NOT NULL
                        THEN S.COD_PDT
                        ELSE LOG.COD_PDT END AS COD_PDT,
                   CASE WHEN S.IDT_SLT_PTB IS NOT NULL
                        THEN S.SLD_DVD
                        ELSE LOG.VLR_EPT END AS VLR_EPT,
                   CONT.CTR_NUMERO_USUARIO AS NUM_CTR_USU,
                   CONT.NUM_PES,
                   PES.COD_CPF_CGC,
                   PES.NOM_PES,
                   LOG.IDC_PRE_DGT,
                   LOG.IDC_REC_OPR,
                   CASE WHEN PC.IDT_PRC IN (16, 25) THEN 'S'
                   ELSE 'N' END AS IDC_MMB,
                   CC.NUM_CTR_CSG,
                   CASE WHEN S.IDT_SLT_PTB IS NOT NULL THEN 'S'
                        ELSE 'N' END AS IDT_PTB,
                   DATE(CONT.CTR_DATA_IMPLANT) AS CTR_DATA_IMPLANT
                 FROM CONTRATO_SIC CONT
                 LEFT JOIN PESSOA PES
                   ON PES.NUM_PES = CONT.NUM_PES
                 LEFT JOIN COMPL_CONTRATO_SIC CCS
                   ON CONT.CTR_NUMERO = CCS.CTR_NUMERO
                 LEFT JOIN COMPL_CONTR_CONVEN CC
                   ON CONT.CTR_NUMERO = CC.CTR_NUMERO
                 LEFT JOIN CONTRATO_PRODUCAO PC
                   ON PC.CTR_NUMERO = CONT.CTR_NUMERO
                 LEFT JOIN LOG_SIC_FEP_GAA LOG
                   ON CONT.CTR_NUMERO_USUARIO = LOG.NUM_CTR_USU
                 LEFT JOIN CONTRATO_PORTADO CP
                   ON CP.CTR_NUMERO = CONT.CTR_NUMERO
                 LEFT JOIN CONTRAT_PORTAB_STC CS
                   ON CP.NUM_PTB = CS.NUM_PTB
                 LEFT JOIN SOLICITACAO_PORTAB S
                   ON CS.IDT_SLT_PTB = S.IDT_SLT_PTB
                   AND CONT.NUM_PES = S.NUM_PES
                WHERE CONT.CTR_NUMERO_USUARIO = :WS-CONTRATO
           END-EXEC.

       0950-FIM. EXIT.

       8888-ROTINA-MSG-DB2.
           MOVE SQLCA      TO L-SQLCA      OF L-PARAMETROS.
           CALL DBZS010S USING L-PARAMETROS.
           IF L-COD-SEV NOT EQUAL ZEROS
              DISPLAY 'ERRO DE ACESSO A ROTINA DBZS010S = ' L-COD-SEV
           ELSE
              DISPLAY 'L-DES-MSG = ' L-DES-MSG
              DISPLAY 'L-COD-MSG = ' L-COD-MSG
           END-IF.

       8888-FIM. EXIT.

       9999-ENCERRA-PROGRAMA.
           DISPLAY 'PROGRAMA ICCP607B CANCELADO'.
           MOVE 3000 TO RETURN-CODE.
           STOP RUN.

       9999-FIM. EXIT.