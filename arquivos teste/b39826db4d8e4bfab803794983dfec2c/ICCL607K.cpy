      ******************************************************************
      * CLIENTE          - BMB - BANCO MERCANTIL DO BRASIL S/A         *
      * SISTEMA          - ICC - INTEGRACAO CENTRAL CESSOES CREDITO    *
      * ANALISTA         - RUMO_GLEISSON JUNIOR                        *
      * PROGRAMADOR      - RUMO_GLEISSON JUNIOR                        *
      * BOOK             - ICCL607K                                    *
      * NOME             - ARQUIVO ENTRADA CONSOLES RECIBOS E LOGS     *
      * TAMANHO          - 1000 BYTES                                  *
      * FORMATO          - FIXO                                        *
      * DATA             - AGOSTO/2025                                  *
      *----------------------------------------------------------------*
      * INI FIM TAM DEC NOME                  D E S C R I C A O        *
      *----------------------------------------------------------------*
      * 001 1000 1000    ICCL313K              AREA TOTAL DA BOOK      *
      ******************************************************************
       01  ICCL607K.
           03 REG-HEADER-607.
              05 H-IDT-EMP              PIC 9(009).
              05 SEP-1                  PIC X(001).
              05 H-NUM-CTR-CSO-CRD      PIC X(012).
              05 SEP-2                  PIC X(001).
              05 H-TIPO-DOCUMENTO       PIC X(001).
              05 SEP-22                 PIC X(001).
              05 H-IDC-VLD              PIC X(001).
              05 FILLER                 PIC X(974).
           03 REG-DETALHE-607 REDEFINES REG-HEADER-607.
              05 IDT-CNL-RLN            PIC X(004).
              05 SEP-3                  PIC X(001).
              05 COD-OGM                PIC X(004).
              05 SEP-4                  PIC X(001).
              05 DTA-MOV                PIC X(010).
              05 SEP-5                  PIC X(001).
              05 DTA-STT-LOG            PIC X(010).
              05 SEP-6                  PIC X(001).
              05 NSU-LOG                PIC X(009).
              05 SEP-7                  PIC X(001).
              05 NSU-COF                PIC X(009).
              05 SEP-8                  PIC X(001).
              05 NUM-DND                PIC X(004).
              05 SEP-9                  PIC X(001).
              05 NUM-CTA                PIC X(009).
              05 SEP-10                 PIC X(001).
              05 COD-PDT                PIC x(004).
              05 SEP-11                 PIC X(001).
              05 VLR-EPT                PIC 9999999999999,99.
              05 SEP-12                 PIC X(001).
              05 NUM-CTR-USU            PIC X(012).
              05 SEP-13                 PIC X(001).
              05 NUM-PES                PIC X(009).
              05 SEP-14                 PIC X(001).
              05 COD-CPF-CGC            PIC X(014).
              05 SEP-15                 PIC X(001).
              05 NOM-PES                PIC X(040).
              05 SEP-16                 PIC X(001).
              05 IDC-PRE-DGT            PIC X(001).
              05 SEP-17                 PIC X(001).
              05 IDC-REC-OPR            PIC X(001).
              05 SEP-18                 PIC X(001).
              05 IDC-MMB                PIC X(001).
              05 SEP-19                 PIC X(001).
              05 NUM-CTR-CSG            PIC X(020).
              05 SEP-20                 PIC X(001).
              05 IDT-PTB                PIC X(001).
              05 SEP-21                 PIC X(001).
              05 CTR-DATA-IMPLANT       PIC X(010).
              05 FILLER                 PIC X(794).