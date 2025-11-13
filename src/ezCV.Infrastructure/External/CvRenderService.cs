using ezCV.Application.External;
using ezCV.Application.Features.CvProcessing.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using PuppeteerSharp;
using PuppeteerSharp.Media;
using System.Text;

namespace ezCV.Infrastructure.External
{
    public class CvRenderService : ICvRenderService
    {
        private readonly ILogger<CvRenderService> _logger;
        private readonly IWebHostEnvironment _environment;

        public CvRenderService(ILogger<CvRenderService> logger, IWebHostEnvironment environment)
        {
            _logger = logger;
            _environment = environment;
        }

        public async Task<string> RenderCvToPdfAsync(CvSubmissionRequest cvData)
        {
            try
            {
                _logger.LogInformation("Bắt đầu render PDF cho template {TemplateId}", cvData.TemplateId);

                // 1. Chọn và tạo HTML template dựa trên templateId
                string htmlContent = GenerateTemplateHtml(cvData.TemplateId, cvData);

                // 2. Tạo PDF từ HTML
                string pdfPath = await ConvertHtmlToPdf(htmlContent, cvData.Profile.FullName);

                _logger.LogInformation("Render PDF thành công: {PdfPath}", pdfPath);
                return pdfPath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi render PDF");
                throw;
            }
        }

        public async Task<string> RenderCvToHtmlAsync(CvSubmissionRequest cvData)
        {
            try
            {
                _logger.LogInformation("Bắt đầu render HTML cho template {TemplateId}", cvData.TemplateId);

                // Chọn và tạo HTML template dựa trên templateId
                string htmlContent = GenerateTemplateHtml(cvData.TemplateId, cvData);

                return htmlContent;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi render HTML");
                throw;
            }
        }

        private string GenerateTemplateHtml(int templateId, CvSubmissionRequest cvData)
        {
            return templateId switch
            {
                1 => GenerateTemplate1(cvData),  // Template Professional
                2 => GenerateTemplate2(cvData),  // Template Modern 
                3 => GenerateTemplate3(cvData),  // Template Interactive (Amber)
                4 => GenerateTemplate4(cvData),  // Template Sky Blue
                5 => GenerateTemplate5(cvData),  // Template Black & White
                6 => GenerateTemplate6(cvData),  // Template 
                _ => GenerateTemplate1(cvData)   // Default
            };
        }

        #region Template Base Methods

        private string GenerateBaseTemplate(CvSubmissionRequest cvData, string css, string bodyContent)
        {
            return $@"
            <!DOCTYPE html>
            <html lang=""vi"">
            <head>
                <meta charset=""UTF-8"" />
                <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
                <title>CV Builder - Template</title>
                <link href=""https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.5.1/css/all.min.css"" rel=""stylesheet"">
                <link rel=""preconnect"" href=""https://fonts.googleapis.com"">
                <link rel=""preconnect"" href=""https://fonts.gstatic.com"" crossorigin>
                <link href=""https://fonts.googleapis.com/css2?family=Roboto:wght@300;400;500;700&display=swap"" rel=""stylesheet"">
                <style>
                    {css}
                </style>
            </head>
            <body>
                {bodyContent}
            </body>
            </html>";
        }

        private string GetCommonPrintStyles()
        {
            return @"
        @@media print {
            body * {
                background: transparent !important;
                color: black !important;
            }
            .cv-container,
            .cv-container * {
                background: white !important;
            }
            .page-header,
            .add-btn-container,
            .print-button,
            .picture-upload-label,
            .dynamic-item:hover,
            .delete-btn,
            .add-btn-container,
            .add-btn {
                display: none !important;
            }
            .preview-pane {
                box-shadow: none !important;
                margin: 0 !important;
                padding: 1.5cm;
                border: none;
                width: 100%;
                height: 100%;
            }
            .dynamic-item:hover {
                background-color: transparent !important;
            }
        }";
        }

        #endregion

        #region Template Methods

        private string GenerateTemplate1(CvSubmissionRequest cvData)
        {
            var profile = cvData.Profile;
            var css = GetTemplate1Styles();
            var body = GetTemplate1Body(profile, cvData);
            return GenerateBaseTemplate(cvData, css, body);
        }

        private string GenerateTemplate2(CvSubmissionRequest cvData)
        {
            var profile = cvData.Profile;
            var css = GetTemplate2Styles();
            var body = GetTemplate2Body(profile, cvData);
            return GenerateBaseTemplate(cvData, css, body);
        }

        private string GenerateTemplate3(CvSubmissionRequest cvData)
        {
            var profile = cvData.Profile;
            var css = GetTemplate3Styles();
            var body = GetTemplate3Body(profile, cvData);
            return GenerateBaseTemplate(cvData, css, body);
        }

        private string GenerateTemplate4(CvSubmissionRequest cvData)
        {
            var profile = cvData.Profile;
            var css = GetTemplate4Styles();
            var body = GetTemplate4Body(profile, cvData);
            return GenerateBaseTemplate(cvData, css, body);
        }

        private string GenerateTemplate5(CvSubmissionRequest cvData)
        {
            var profile = cvData.Profile;
            var css = GetTemplate5Styles();
            var body = GetTemplate5Body(profile, cvData);
            return GenerateBaseTemplate(cvData, css, body);
        }

        private string GenerateTemplate6(CvSubmissionRequest cvData)
        {
            var profile = cvData.Profile;
            var css = GetTemplate6Styles();
            var body = GetTemplate6Body(profile, cvData);
            return GenerateBaseTemplate(cvData, css, body);
        }

        #endregion

        #region Template Styles

        private string GetTemplate1Styles()
        {
            return $@"
        :root {{
            --cv-padding: 1.5rem;
            --gray-text: #555;
            --dark-text: #222;
            --light-gray-text: #777;
            --icon-bg: #4A4A4A;
            --border-color: #e8e8e8;
            --hover-bg: #f0f8ff;
        }}

        body {{
            background-color: white;
            font-family: 'Roboto', sans-serif;
            color: var(--gray-text);
            font-size: 9pt;
            font-weight: 300;
            margin: 0;
            padding: 0;
        }}

        /* Layout & Common Elements */
        .page-container {{
            display: flex;
            flex-direction: column;
            align-items: center;
            padding: 0;
            margin: 0;
            background: white;
            min-height: 100vh;
        }}

        .preview-pane {{
            background-color: white;
            box-shadow: 0 10px 15px -3px rgba(0, 0, 0, 0.1), 0 4px 6px -2px rgba(0, 0, 0, 0.05);
            width: 210mm;
            min-height: 297mm;
            padding: var(--cv-padding);
            margin: 0;
            display: flex;
            flex-direction: column;
            overflow: hidden;
            border: 1px solid #e0e0e0;
        }}

        {GetCommonPrintStyles()}

        /* Header */
        .preview-header {{
            display: flex;
            align-items: center;
            padding-bottom: 1rem;
            margin-bottom: 1rem;
            border-bottom: 1px solid var(--border-color);
            min-height: 120px;
        }}

        #preview-photo {{
            width: 80px;
            height: 80px;
            object-fit: cover;
            border-radius: 50%;
            flex-shrink: 0;
            margin-right: 1.5rem;
        }}

        .header-main {{
            flex-grow: 1;
            text-align: left;
            min-width: 0;
        }}

        #preview-name {{
            font-size: 1.5rem;
            font-weight: 700;
            color: var(--dark-text);
            letter-spacing: 0.1em;
            text-transform: uppercase;
            margin: 0;
            line-height: 1.2;
        }}

        #preview-title {{
            font-size: 0.9rem;
            font-weight: 400;
            color: var(--gray-text);
            letter-spacing: 0.1em;
            text-transform: uppercase;
            margin-top: 0.25rem;
            margin-bottom: 0;
        }}

        .header-divider {{
            width: 1px;
            background-color: var(--border-color);
            align-self: stretch;
            margin: 0 1rem;
            flex-shrink: 0;
        }}

        .contact-info {{
            flex-shrink: 0;
            min-width: 200px;
        }}

        .contact-list {{
            list-style: none;
            padding: 0;
            margin: 0;
            display: flex;
            flex-direction: column;
            gap: 0.25rem;
        }}

        .contact-item {{
            position: relative;
            display: flex;
            align-items: center;
            justify-content: flex-end;
            gap: 0.5rem;
        }}

        .contact-value {{
            text-align: right;
            font-size: 8pt;
            margin: 0;
            line-height: 1.2;
            white-space: nowrap;
            overflow: hidden;
            text-overflow: ellipsis;
        }}

        .contact-item .icon-wrapper {{
            flex-shrink: 0;
            width: 20px;
            height: 20px;
            background-color: var(--icon-bg);
            border-radius: 50%;
            display: flex;
            align-items: center;
            justify-content: center;
        }}

        .contact-item .icon-wrapper i {{
            font-size: 9px;
            color: white;
        }}

        /* Body & Columns */
        .preview-body {{
            display: flex;
            flex-grow: 1;
            gap: 1.5rem;
            min-height: 0;
            overflow: hidden;
        }}

        .left-column {{
            width: 35%;
            display: flex;
            flex-direction: column;
            gap: 1rem;
            min-height: 0;
        }}

        .main-divider {{
            width: 1px;
            background-color: var(--border-color);
            flex-shrink: 0;
        }}

        .right-column {{
            width: 65%;
            display: flex;
            flex-direction: column;
            gap: 1rem;
            min-height: 0;
        }}

        /* Generic Section Styling */
        .cv-section {{
            display: flex;
            flex-direction: column;
            min-height: 0;
        }}

        .section-title {{
            font-size: 0.8rem;
            font-weight: 700;
            color: var(--dark-text);
            letter-spacing: 0.1em;
            text-transform: uppercase;
            padding-bottom: 0.25rem;
            margin-bottom: 0.75rem;
            border-bottom: 1px solid var(--border-color);
        }}

        /* Section Specifics */
        #about-preview p {{
            line-height: 1.4;
            margin: 0;
            font-size: 8.5pt;
        }}

        .list-container {{
            display: flex;
            flex-direction: column;
            gap: 0.75rem;
            min-height: 0;
            overflow: hidden;
        }}

        .job-item .job-header {{
            margin-bottom: 0.1rem;
        }}

        .job-item .job-title {{
            font-weight: 700;
            color: var(--dark-text);
            font-size: 9pt;
            margin: 0;
            line-height: 1.2;
        }}

        .job-item .job-date {{
            font-style: italic;
            font-size: 8pt;
            margin: 0;
            line-height: 1.2;
        }}

        .job-item .job-company {{
            font-weight: 500;
            font-size: 8.5pt;
            margin-bottom: 0.25rem;
            line-height: 1.2;
        }}

        .job-item .job-description {{
            line-height: 1.4;
            white-space: pre-wrap;
            font-size: 8pt;
            margin: 0;
            padding-left: 0 !important;
            margin-left: 0 !important;
            text-indent: 0 !important;
        }}

        .edu-item .edu-degree,
        .cert-item .cert-title {{
            font-weight: 700;
            color: var(--dark-text);
            font-size: 8.5pt;
            margin: 0;
            line-height: 1.2;
        }}

        .edu-item .edu-institution,
        .cert-item .cert-date {{
            font-size: 8pt;
            margin: 0;
            line-height: 1.2;
        }}

        .edu-item .edu-date {{
            font-size: 7.5pt;
            margin-top: 1px;
            line-height: 1.2;
        }}

        .skill-item p {{
            margin-bottom: 0.1rem;
            font-size: 8.5pt;
            line-height: 1.2;
        }}

        .interest-item {{
            background-color: #f0f0f0;
            color: var(--dark-text);
            padding: 0.15rem 0.5rem;
            border-radius: 3px;
            display: inline-block;
            text-align: center;
            font-size: 8pt;
        }}

        .interest-list {{
            display: flex;
            flex-wrap: wrap;
            gap: 0.5rem;
            padding-top: 0.25rem;
        }}

        /* Remove unnecessary elements for PDF */
        .print-button,
        .dynamic-item:hover,
        .delete-btn,
        .add-btn-container,
        .add-btn {{
            display: none !important;
        }}

        /* Ensure content fits in one page */
        .cv-section:last-child {{
            flex-grow: 1;
        }}

        .list-container {{
            max-height: none;
        }}

        /* Ẩn section không có nội dung */
        .empty-section {{
            display: none !important;
        }}";
        }

        private string GetTemplate2Styles()
        {
            return $@"
        :root {{
            --primary-color: #f37021;
            --text-dark: #333;
            --text-light: #555;
            --border-color: #e0e0e0;
            --hover-bg: #fffaf0;
        }}

        body {{
            background-color: white;
            font-family: 'Roboto', sans-serif;
            color: var(--text-light);
            font-size: 9pt;
            line-height: 1.5;
            margin: 0;
            padding: 0;
        }}

        .page-container {{
            max-width: 900px;
            margin: 0 auto;
            padding: 0;
        }}

        .cv-container {{
            background-color: white;
            box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
            display: flex;
            margin: 0;
            width: 210mm;
            height: 297mm;
            overflow: hidden;
        }}

        /* --- Sidebar (Left Column) --- */
        .cv-sidebar {{
            width: 35%;
            padding: 1.5rem;
            background: #f9f9f9;
            min-height: 0;
            overflow: hidden;
        }}

        .profile-picture-container {{
            position: relative;
            margin: 0 auto 1.5rem auto;
            width: 120px;
            height: 120px;
        }}

        #profile-picture {{
            width: 100%;
            height: 100%;
            object-fit: cover;
            border-radius: 50%;
        }}

        .sidebar-section .section-title {{
            display: flex;
            align-items: center;
            text-align: center;
            font-weight: 700;
            color: var(--primary-color);
            text-transform: uppercase;
            font-size: 0.9rem;
            margin: 1.5rem 0 0.75rem 0;
        }}

        .sidebar-section .section-title::before,
        .sidebar-section .section-title::after {{
            content: '';
            flex: 1;
            border-bottom: 1px solid var(--border-color);
        }}

        .sidebar-section .section-title:not(:empty)::before {{
            margin-right: .5em;
        }}

        .sidebar-section .section-title:not(:empty)::after {{
            margin-left: .5em;
        }}

        #contact-list {{
            list-style: none;
            padding: 0;
            margin: 0;
        }}

        #contact-list .contact-item {{
            display: flex;
            align-items: center;
            margin-bottom: 0.5rem;
        }}

        #contact-list .contact-icon {{
            background-color: var(--primary-color);
            color: white;
            width: 25px;
            height: 25px;
            border-radius: 50%;
            display: inline-flex;
            justify-content: center;
            align-items: center;
            margin-right: 10px;
            flex-shrink: 0;
            font-size: 0.7rem;
        }}

        #contact-list .contact-value {{
            flex-grow: 1;
            font-size: 8.5pt;
        }}

        /* --- Main Content (Right Column) --- */
        .cv-main {{
            width: 65%;
            padding: 2rem;
            border-left: 1px solid var(--border-color);
            min-height: 0;
            overflow: hidden;
        }}

        .main-header {{
            margin-bottom: 1.5rem;
        }}

        #cv-name {{
            font-size: 2rem;
            font-weight: 900;
            color: var(--text-dark);
            margin-bottom: 0.1rem;
            line-height: 1.1;
        }}

        #cv-title {{
            font-size: 1.1rem;
            font-weight: 700;
            color: var(--primary-color);
            margin-bottom: 0;
        }}

        .main-section .section-title {{
            font-weight: 700;
            color: var(--primary-color);
            text-transform: uppercase;
            font-size: 0.9rem;
            margin-bottom: 1rem;
            padding-bottom: 0.25rem;
            border-bottom: 2px solid var(--primary-color);
            display: inline-block;
        }}

        .list-container {{
            display: flex;
            flex-direction: column;
            gap: 1rem;
            min-height: 0;
        }}
        
        .list-item {{
            position: relative;
        }}

        .item-title {{
            font-size: 0.9rem;
            font-weight: 700;
            color: var(--text-dark);
            margin: 0;
            line-height: 1.2;
        }}

        .item-subtitle {{
            font-weight: 500;
            margin-bottom: 0.25rem;
            font-size: 8.5pt;
            line-height: 1.2;
        }}

       .item-description {{
            padding-left: 1rem;
            list-style: none;
            margin: 0;
        }}

        .item-description li {{
            position: relative;
            margin-bottom: 0.15rem;
            font-size: 9pt;
            line-height: 1.3;
        }}

        .item-description li::before {{
            content: '•';
            position: absolute;
            left: -0.8rem;
            color: var(--primary-color);
            font-weight: bold;
            font-size: 1.2em;
        }}

        /* --- Remove interactive elements for PDF --- */
        .print-button,
        .picture-upload-label,
        .dynamic-item:hover,
        .delete-btn,
        .add-btn-container,
        .add-btn {{
            display: none !important;
        }}

        {GetCommonPrintStyles()}

        @@media print {{
            .page-container {{
                margin: 0;
                max-width: 100%;
            }}
            .cv-container {{
                box-shadow: none;
                margin: 0;
            }}
        }}";
        }

        private string GetTemplate3Styles()
        {
            return @"
        * {
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }
        
        :root {
            --primary-color: #b45309;
            --dark-text: #292524;
            --gray-text: #57534e;
            --light-gray: #d6d3d1;
            --border-color: #e7e5e4;
        }
        
        body {
            font-family: 'Inter', sans-serif;
            background-color: white !important;
            color: var(--gray-text);
            font-size: 9pt;
            line-height: 1.3;
            margin: 0 !important;
            padding: 0 !important;
            width: 210mm;
            height: 297mm;
            overflow: hidden;
        }
        
        .cv-container {
            width: 210mm;
            height: 297mm;
            background: white;
            margin: 0 auto;
            padding: 1rem 1.5rem;
            overflow: hidden;
            position: relative;
        }
        
        " + GetCommonPrintStyles() + @"
        
        /* Header */
        .cv-header {
            text-align: center;
            margin-bottom: 0.8rem;
            padding-bottom: 0.5rem;
            border-bottom: 2px solid var(--primary-color);
        }
        
        .cv-name {
            font-size: 1.4rem;
            font-weight: 700;
            color: var(--dark-text);
            text-transform: uppercase;
            letter-spacing: 0.05em;
            margin-bottom: 0.1rem;
            line-height: 1.1;
        }
        
        .cv-title {
            font-size: 0.8rem;
            font-weight: 500;
            color: var(--primary-color);
            text-transform: uppercase;
            letter-spacing: 0.1em;
        }
        
        /* Contact Info */
        .contact-grid {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(160px, 1fr));
            gap: 0.2rem;
            margin-bottom: 0.8rem;
            font-size: 8pt;
        }
        
        .contact-item {
            display: flex;
            align-items: center;
            gap: 0.3rem;
        }
        
        .contact-icon {
            color: var(--primary-color);
            width: 12px;
            text-align: center;
            font-size: 0.65rem;
        }
        
        /* Main Layout */
        .cv-main {
            display: grid;
            grid-template-columns: 35% 65%;
            gap: 0.8rem;
            height: calc(297mm - 7rem);
            overflow: hidden;
        }
        
        .section-title {
            font-size: 0.8rem;
            font-weight: 600;
            color: var(--dark-text);
            text-transform: uppercase;
            letter-spacing: 0.1em;
            margin-bottom: 0.4rem;
            padding-bottom: 0.15rem;
            border-bottom: 1px solid var(--border-color);
        }
        
        /* Columns */
        .left-column {
            display: flex;
            flex-direction: column;
            gap: 0.8rem;
            padding-right: 0.3rem;
            overflow: hidden;
        }
        
        .right-column {
            display: flex;
            flex-direction: column;
            gap: 0.8rem;
            border-left: 1px solid var(--border-color);
            padding-left: 0.8rem;
            overflow: hidden;
        }
        
        /* GIỚI THIỆU - QUAN TRỌNG: ĐẢM BẢO HIỂN THỊ ĐẦY ĐỦ */
        .summary-section {
            flex-shrink: 0;
        }
        
        .summary-content {
            font-size: 8.5pt !important;
            line-height: 1.4 !important;
            text-align: justify;
            white-space: pre-line !important;
            word-wrap: break-word !important;
            overflow: visible !important;
            max-height: none !important;
        }
        
        /* List Items */
        .list-item {
            margin-bottom: 0.5rem;
            page-break-inside: avoid;
        }
        
        .item-header {
            display: flex;
            justify-content: space-between;
            align-items: flex-start;
            margin-bottom: 0.1rem;
            flex-wrap: nowrap;
        }
        
        .item-title {
            font-weight: 600;
            color: var(--dark-text);
            font-size: 9pt;
            line-height: 1.2;
            flex: 1;
        }
        
        .item-date {
            font-size: 8pt;
            color: var(--gray-text);
            flex-shrink: 0;
            line-height: 1.2;
            text-align: right;
            min-width: 70px;
            white-space: nowrap;
            margin-left: 0.5rem;
        }
        
        .item-subtitle {
            font-style: italic;
            color: var(--gray-text);
            font-size: 8.5pt;
            margin-bottom: 0.15rem;
            line-height: 1.2;
        }
        
        .item-description {
            font-size: 8pt;
            line-height: 1.3;
            white-space: pre-line;
        }
        
        /* Skills & Interests */
        .skills-list {
            display: flex;
            flex-direction: column;
            gap: 0.15rem;
        }
        
        .skill-item {
            padding: 0.02rem 0;
            font-size: 8pt;
        }
        
        .interests-list {
            display: flex;
            flex-wrap: wrap;
            gap: 0.3rem;
            margin-bottom: 0.2rem;
        }
        
        .interest-tag {
            background: var(--light-gray);
            padding: 0.2rem 0.4rem;
            border-radius: 10px;
            font-size: 8pt;
            display: inline-block;
        }

        /* Row-Col Layout */
        .row {
            display: flex;
            flex-wrap: wrap;
            margin-right: -0.3rem;
            margin-left: -0.3rem;
        }
        
        .g-3 {
            gap: 0.6rem;
        }
        
        .col-6 {
            flex: 0 0 auto;
            width: 50%;
            padding-right: 0.3rem;
            padding-left: 0.3rem;
        }

        .no-print,
        .print-button,
        .dynamic-item:hover,
        .delete-btn,
        .add-btn-container,
        .add-btn,
        .loading-overlay,
        .contact-modal,
        .option-panel,
        .submit-section {
            display: none !important;
        }

        .cv-container * {
            background: transparent !important;
        }
        
        .left-column,
        .right-column {
            background: white !important;
        }

        .left-column {
            min-height: 0;
            flex: 1;
        }
        
        .summary-content {
            max-height: none !important;
            overflow: visible !important;
        }";
        }

        private string GetTemplate4Styles()
        {
            return @"
        :root {
            --primary-color: #0ea5e9;
            --text-dark: #1f2937;
            --text-medium: #4b5563;
            --text-light: #6b7280;
            --bg-light: #ffffff;
            --border-color: #e5e7eb;
        }

        body {
            font-family: 'Roboto', sans-serif;
            background-color: white;
            color: var(--text-medium);
            margin: 0;
            padding: 0;
            -webkit-font-smoothing: antialiased;
            -moz-osx-font-smoothing: grayscale;
        }

        #resume-container {
            max-width: 210mm;
            min-height: 297mm;
            margin: 0 auto;
            background-color: white;
            box-shadow: 0 0 10px rgba(0,0,0,0.1);
            padding: 2rem;
        }

        @@media (min-width: 640px) {
            #resume-container {
                padding: 2.5rem;
            }
        }

        header {
            text-align: center;
            margin-bottom: 2.5rem;
        }

        header h1 {
            font-size: 2.25rem;
            font-weight: 700;
            letter-spacing: -0.025em;
            color: var(--text-dark);
            margin: 0;
        }

        header h2 {
            font-size: 1.5rem;
            color: var(--text-light);
            margin-top: 0.5rem;
        }

        .section {
            display: grid;
            grid-template-columns: 1fr;
            gap: 8px;
            padding: 24px 0 0 0;
            border-top: 1px solid var(--border-color);
        }

        main > .section:first-of-type {
            border-top: none;
            padding-top: 0;
        }

        @@media (min-width: 768px) {
            .section {
                grid-template-columns: 1fr 3fr;
                gap: 1.5rem;
            }
        }

        .section-title {
            font-size: 0.875rem;
            font-weight: 700;
            text-transform: uppercase;
            color: var(--primary-color);
            letter-spacing: 0.05em;
            margin: 0;
        }

        @@media (min-width: 768px) {
            .section-title {
                text-align: right;
                border-right: 2px solid var(--border-color);
                padding-right: 1.5rem;
            }
        }

        .section-content {
            line-height: 1.625;
        }

        .contact-grid {
            display: grid;
            grid-template-columns: 1fr;
            gap: 0.5rem 2rem;
        }

        @@media (min-width: 640px) {
            .contact-grid {
                grid-template-columns: 1fr 1fr;
            }
        }

        .contact-item {
            display: flex;
            align-items: center;
        }

        .contact-item i {
            color: var(--primary-color);
            width: 1rem;
            margin-right: 0.75rem;
        }

        .skills-list {
            display: flex;
            flex-wrap: wrap;
            gap: 1rem;
        }

        .experience-list {
            display: flex;
            flex-direction: column;
            gap: 1.5rem;
        }

        .experience-item-header {
            display: flex;
            justify-content: space-between;
            align-items: flex-start;
            gap: 1rem;
        }

        .experience-item-header h3 {
            font-size: 1.125rem;
            font-weight: 600;
            color: var(--text-dark);
            margin:0;
        }

        .experience-item-header p {
            font-weight: 500;
            color: var(--text-medium);
            margin: 0;
        }

        .experience-item-date {
            font-size: 0.75rem;
            color: var(--text-light);
            white-space: nowrap;
            text-align: right;
        }

        .responsibilities-list {
            list-style: none;
            padding-left: 0;
            margin-top: 0.5rem;
            display: flex;
            flex-direction: column;
            gap: 0.5rem;
        }

        .responsibility-item {
            display: flex;
            align-items: flex-start;
            gap: 0.5rem;
        }

        .responsibility-item::before {
            content: '•';
            color: var(--primary-color);
            margin-top: 0.15em;
            font-size: 1.25em;
            line-height: 1;
        }

        " + GetCommonPrintStyles() + @"

        /* Remove interactive elements */
        .print-button,
        .dynamic-item:hover,
        .delete-btn,
        .add-btn-container,
        .add-btn {
            display: none !important;
        }

        /* Hide empty sections */
        .empty-section {
            display: none !important;
        }";
        }

        private string GetTemplate5Styles()
        {
            return $@"
        :root {{
            --primary-color: #f37021;
            --text-dark: #333;
            --text-light: #555;
            --border-color: #e0e0e0;
            --hover-bg: #fffaf0;
            --danger-color: #ef4444;
            --icon-bg: #4A4A4A;
        }}

        body {{
            font-family: 'Inter', sans-serif;
            -webkit-font-smoothing: antialiased;
            -moz-osx-font-smoothing: grayscale;
            background-color: #f5f5f4;
            margin: 0;
            padding: 2rem;
        }}

        {GetCommonPrintStyles()}

        @@media print {{
            body {{
                -webkit-print-color-adjust: exact;
                print-color-adjust: exact;
                background-color: white;
                padding: 0;
                margin: 0;
            }}
            .cv-container {{
                box-shadow: none !important;
                margin: 0 !important;
                max-width: 100% !important;
                border: none !important;
                padding: 1.5cm !important;
            }}
        }}

        .cv-container {{
            background-color: white;
            box-shadow: 0 10px 15px -3px rgba(0, 0, 0, 0.1);
            padding: 2rem;
            max-width: 1000px;
            margin: 0 auto;
        }}

        .section-title {{
            font-size: 1.125rem;
            font-weight: 900;
            letter-spacing: 0.1em;
            text-transform: uppercase;
            color: #1c1917;
            margin-bottom: 1rem;
        }}

        .cv-header {{
            border-bottom: 2px solid #000;
            padding-bottom: 1rem;
            margin-bottom: 1rem;
            display: flex;
            justify-content: space-between;
            align-items: flex-start;
        }}

        .cv-name {{
            font-size: 3rem;
            font-weight: 900;
            color: #000;
            margin-bottom: 0.25rem;
            line-height: 1;
        }}

        .cv-position {{
            font-size: 1.25rem;
            color: #57534e;
            font-weight: 500;
            letter-spacing: 0.1em;
            margin-bottom: 0;
        }}

        .header-left {{
            flex: 2;
        }}

        .contact-column {{
            border-left: 2px solid #000;
            padding-left: 1.5rem;
            flex: 1;
        }}

        .contact-item {{
            display: flex;
            align-items: center;
            margin-bottom: 0.5rem;
            font-size: 0.875rem;
        }}

        .contact-icon {{
            width: 24px;
            height: 24px;
            margin-right: 0.75rem;
            flex-shrink: 0;
            background-color: var(--icon-bg);
            border-radius: 50%;
            display: flex;
            align-items: center;
            justify-content: center;
        }}

        .contact-icon i {{
            font-size: 11px;
            color: white;
        }}

        .main-content {{
            margin-top: 2rem;
            display: flex;
            gap: 2rem;
        }}

        .left-column {{
            flex: 2;
            padding-right: 1rem;
        }}

        .right-column {{
            flex: 1;
            border-left: 2px solid #000;
            padding-left: 1.5rem;
        }}

        .experience-item {{
            margin-bottom: 1.5rem;
        }}

        .item-header {{
            margin-bottom: 0.5rem;
        }}

        .item-title {{
            font-weight: 700;
            color: #000;
            margin-bottom: 0.25rem;
            font-size: 1rem;
        }}

        .item-company {{
            font-weight: 600;
            color: #44403c;
            margin-bottom: 0.25rem;
            font-size: 0.9rem;
        }}

        .item-period {{
            font-size: 0.875rem;
            color: #78716c;
            font-weight: 500;
        }}

        .item-description {{
            padding-left: 1rem;
            margin: 0;
            list-style: none;
        }}

        .item-description li {{
            margin-bottom: 0.25rem;
            position: relative;
            padding-left: 1rem;
            font-size: 0.875rem;
            line-height: 1.4;
        }}

        .item-description li::before {{
            content: '•';
            position: absolute;
            left: 0;
            color: #000;
            font-weight: bold;
        }}

        .cert-item {{
            margin-bottom: 1rem;
        }}

        .skills-list {{
            list-style: none;
            padding: 0;
            margin: 0;
        }}

        .skills-list li {{
            margin-bottom: 0.5rem;
            font-size: 0.875rem;
        }}

        /* Responsive */
        @@media (max-width: 768px) {{
            .cv-header {{
                flex-direction: column;
            }}
            .contact-column {{
                border-left: none;
                border-top: 2px solid #000;
                padding-left: 0;
                padding-top: 1rem;
                margin-top: 1rem;
                width: 100%;
            }}
            .main-content {{
                flex-direction: column;
            }}
            .right-column {{
                border-left: none;
                border-top: 2px solid #000;
                padding-left: 0;
                padding-top: 2rem;
                margin-top: 2rem;
            }}
            .cv-name {{
                font-size: 2rem;
            }}
        }}

        /* Remove interactive elements */
        .print-button,
        .dynamic-item:hover,
        .delete-btn,
        .add-btn-container,
        .add-btn {{
            display: none !important;
        }}";
        }

        private string GetTemplate6Styles()
        {
            return @"
        :root {
            --primary-color: #DD704A;
            --text-dark: #333;
            --text-light: #555;
            --border-color: #e0e0e0;
            --hover-bg: #fef5f2;
            --danger-color: #ef4444;
        }

        * {
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }

        body {
            background-color: white !important;
            font-family: 'Roboto', sans-serif;
            color: var(--text-light);
            font-size: 11pt;
            line-height: 1.6;
            margin: 0 !important;
            padding: 0 !important;
            width: 210mm;
            height: 297mm;
            overflow: hidden;
        }

        .cv-container {
            background-color: white !important;
            box-shadow: none !important;
            border: none !important;
            margin: 0 auto;
            width: 210mm;
            min-height: 297mm;
            padding: 2.5rem;
            max-width: 100%;
            box-sizing: border-box;
            position: relative;
        }

        /* Header */
        .cv-header {
            display: flex;
            justify-content: space-between;
            align-items: flex-start;
            margin-bottom: 2rem;
            border-bottom: 1px solid var(--border-color);
            padding-bottom: 1.5rem;
        }

        .cv-name {
            font-size: 2.5rem;
            font-weight: 900;
            color: var(--text-dark);
            margin: 0;
            line-height: 1.1;
        }

        .cv-title {
            font-size: 1.3rem;
            font-weight: 700;
            color: var(--primary-color);
            margin: 0.25rem 0 0 0;
        }

        .header-right {
            display: flex;
            flex-direction: column;
            align-items: flex-end;
        }

        .contact-list {
            list-style: none;
            padding: 0;
            margin: 0;
            text-align: right;
            font-size: 10pt;
        }

        .contact-list li {
            position: relative;
            display: flex;
            justify-content: flex-end;
            align-items: center;
            margin-bottom: 0.3rem;
            padding: 2px 0;
        }

        .contact-list li:last-child {
            margin-bottom: 0;
        }

        .contact-value {
           margin-right: 0.5rem;
           min-height: 1.2em;
        }

        .contact-icon {
            color: var(--primary-color);
            width: 16px;
            text-align: center;
            font-size: 0.9rem;
        }

        /* Sections */
        .cv-section {
            margin-bottom: 2rem;
        }

        .section-title {
            font-size: 1.2rem;
            font-weight: 900;
            text-transform: uppercase;
            color: var(--text-dark);
            margin: 0 0 1rem 0;
            padding-bottom: 0.5rem;
            border-bottom: 2px solid var(--text-dark);
        }

        /* Entry Items (Experience & Education) */
        .entry {
            position: relative;
            margin-bottom: 1.5rem;
        }

        .entry-header {
            display: flex;
            justify-content: space-between;
            align-items: flex-start;
            margin-bottom: 0.25rem;
        }

        .entry-title {
            font-size: 1rem;
            font-weight: 700;
            color: var(--primary-color);
            margin: 0;
        }

        .entry-company {
            font-size: 1rem;
            font-weight: 700;
            color: var(--text-dark);
            margin: 0;
        }

        .entry-date {
            color: var(--text-light);
            font-weight: 500;
            font-size: 10pt;
            flex-shrink: 0;
            margin-left: 1rem;
            white-space: nowrap;
        }

        .entry-description {
            padding-left: 1rem;
            list-style: none;
            margin: 0.5rem 0 0 0;
        }

        .entry-description li {
            position: relative;
            margin-bottom: 0.25rem;
            font-size: 10pt;
            line-height: 1.4;
        }

        .entry-description li::before {
            content: '•';
            position: absolute;
            left: -1rem;
            color: var(--text-dark);
            font-weight: bold;
        }

        /* Remove all interactive elements */
        .option-panel,
        .submit-section,
        .loading-overlay,
        .contact-modal,
        .add-btn,
        .delete-btn,
        .dynamic-item:hover,
        [contenteditable],
        .editable-field,
        .print-button {
            display: none !important;
        }

        " + GetCommonPrintStyles() + @"

        @@media print {
            body {
                background-color: white !important;
                padding: 0 !important;
                margin: 0 !important;
                -webkit-print-color-adjust: exact;
                print-color-adjust: exact;
            }
            .cv-container {
                box-shadow: none !important;
                margin: 0 !important;
                width: 100% !important;
                height: 100% !important;
                padding: 1.5cm !important;
            }
        }

        @@media (max-width: 768px) {
            body {
                padding: 1rem !important;
            }
            .cv-header {
                flex-direction: column;
            }
            .header-right {
                align-items: flex-start;
                margin-top: 1rem;
            }
            .contact-list {
                text-align: left;
            }
            .contact-list li {
                justify-content: flex-start;
            }
            .contact-list .contact-value {
                margin-right: 0;
                margin-left: 0.5rem;
                order: 2;
            }
            .contact-list .contact-icon {
                order: 1;
            }
        }";
        }

        #endregion

        #region Template Body Methods

        private string GetTemplate1Body(CvProfileRequest profile, CvSubmissionRequest cvData)
        {
            var educationHtml = RenderEducationForTemplate1(cvData.Educations);
            var skillsHtml = RenderSkillsForTemplate1(cvData.Skills);
            var certificatesHtml = RenderCertificatesForTemplate1(cvData.Certificates);
            var referencesHtml = RenderReferencesForTemplate1(cvData.References);
            var interestsHtml = RenderInterestsForTemplate1(cvData.Hobbies);
            var experiencesHtml = RenderWorkExperiencesForTemplate1(cvData.WorkExperiences);
            var projectsHtml = RenderProjectsForTemplate1(cvData.Projects);

            return $@"
        <div class=""page-container"">
            <div class=""preview-pane"">
                <!-- Header -->
                <header class=""preview-header"">
                    <img id=""preview-photo"" src=""{EscapeHtml(profile.AvatarUrl ?? profile.AvatarUrl ?? "")}"" alt=""Profile Photo"" style=""width: 80px; height: 80px; object-fit: cover; border-radius: 50%;"">
                    <div class=""header-main"">
                        <h1 id=""preview-name"">{EscapeHtml(profile.FullName)}</h1>
                        <h2 id=""preview-title"">{EscapeHtml(profile.JobTitle)}</h2>
                    </div>
                    <div class=""header-divider""></div>
                    <div class=""contact-info"">
                        <ul id=""contact-list"" class=""contact-list"">
                            {RenderContactForTemplate1(profile)}
                        </ul>
                    </div>
                </header>

                <!-- Body -->
                <main class=""preview-body"">
                    <!-- Left Column -->
                    <div class=""left-column"">
                        {(string.IsNullOrEmpty(educationHtml) ? "" : $@"
                        <section class=""cv-section"">
                            <h3 class=""section-title"">Học vấn</h3>
                            <div id=""education-list"" class=""list-container"">
                                {educationHtml}
                            </div>
                        </section>")}

                        {(string.IsNullOrEmpty(skillsHtml) ? "" : $@"
                        <section class=""cv-section"">
                            <h3 class=""section-title"">Kỹ năng</h3>
                            <div id=""skills-list"" class=""list-container"">
                                {skillsHtml}
                            </div>
                        </section>")}

                        {(string.IsNullOrEmpty(certificatesHtml) ? "" : $@"
                        <section class=""cv-section"">
                            <h3 class=""section-title"">Chứng chỉ</h3>
                            <div id=""certificates-list"" class=""list-container"">
                                {certificatesHtml}
                            </div>
                        </section>")}

                        {(string.IsNullOrEmpty(referencesHtml) ? "" : $@"
                        <section class=""cv-section"">
                            <h3 class=""section-title"">Người tham chiếu</h3>
                            <div id=""references-list"" class=""list-container"">
                                {referencesHtml}
                            </div>
                        </section>")}

                        {(string.IsNullOrEmpty(interestsHtml) ? "" : $@"
                        <section class=""cv-section"">
                            <h3 class=""section-title"">Sở thích</h3>
                            <div id=""interests-list"" class=""interest-list"">
                                {interestsHtml}
                            </div>
                        </section>")}
                    </div>

                    <div class=""main-divider""></div>

                    <!-- Right Column -->
                    <div class=""right-column"">
                        {(string.IsNullOrEmpty(profile.Summary) ? "" : $@"
                        <section class=""cv-section"">
                            <h3 class=""section-title"">Giới thiệu</h3>
                            <div id=""about-preview"">
                                <p>{EscapeHtml(profile.Summary)}</p>
                            </div>
                        </section>")}

                        {(string.IsNullOrEmpty(experiencesHtml) ? "" : $@"
                        <section class=""cv-section"">
                            <h3 class=""section-title"">Kinh nghiệm làm việc</h3>
                            <div id=""experience-list"" class=""list-container"">
                                {experiencesHtml}
                            </div>
                        </section>")}

                        {(string.IsNullOrEmpty(projectsHtml) ? "" : $@"
                        <section class=""cv-section"">
                            <h3 class=""section-title"">Dự án</h3>
                            <div id=""projects-list"" class=""list-container"">
                                {projectsHtml}
                            </div>
                        </section>")}
                    </div>
                </main>
            </div>
        </div>";
        }

        private string GetTemplate2Body(CvProfileRequest profile, CvSubmissionRequest cvData)
        {
            return $@"
        <div class=""page-container"">
            <div class=""cv-container"">
                <!-- Sidebar -->
                <aside class=""cv-sidebar"">
                    <div class=""profile-picture-container"">
                        <img id=""profile-picture"" src=""{EscapeHtml(profile.AvatarUrl ?? "")}"" alt=""Profile Picture"">
                    </div>

                    <section class=""sidebar-section"">
                        <ul id=""contact-list"">
                            {RenderContactForTemplate2(profile)}
                        </ul>
                    </section>

                    {(string.IsNullOrWhiteSpace(profile.Summary) ? "" : $@"
                    <section class=""sidebar-section"">
                        <h3 class=""section-title"">GIỚI THIỆU</h3>
                        <p style=""font-size: 8.5pt; line-height: 1.4; margin: 0;"">{EscapeHtml(profile.Summary)}</p>
                    </section>
                    ")}

                    {(cvData.Skills == null || !cvData.Skills.Any() ? "" : $@"
                    <section class=""sidebar-section"">
                        <h3 class=""section-title"">KỸ NĂNG</h3>
                        <div id=""skills-list"" class=""list-container"">
                            {RenderSkillsForTemplate2(cvData.Skills)}
                        </div>
                    </section>
                    ")}
                </aside>

                <!-- Main Content -->
                <main class=""cv-main"">
                    <header class=""main-header"">
                        <h1 id=""cv-name"">{EscapeHtml(profile.FullName)}</h1>
                        <h2 id=""cv-title"">{EscapeHtml(profile.JobTitle)}</h2>
                    </header>

                    {(cvData.Educations == null || !cvData.Educations.Any() ? "" : $@"
                    <section class=""main-section"">
                        <h3 class=""section-title"">HỌC VẤN</h3>
                        <div id=""education-list"" class=""list-container"">
                            {RenderEducationForTemplate2(cvData.Educations)}
                        </div>
                    </section>
                    ")}

                    {(cvData.WorkExperiences == null || !cvData.WorkExperiences.Any() ? "" : $@"
                    <section class=""main-section"" style=""margin-top: 1.5rem;"">
                        <h3 class=""section-title"">KINH NGHIỆM LÀM VIỆC</h3>
                        <div id=""experience-list"" class=""list-container"">
                            {RenderWorkExperiencesForTemplate2(cvData.WorkExperiences)}
                        </div>
                    </section>
                    ")}
                </main>
            </div>
        </div>";
        }

        private string GetTemplate3Body(CvProfileRequest profile, CvSubmissionRequest cvData)
        {
            var summary = profile.Summary ?? "";

            return $@"
            <div class=""cv-container"">
                <header class=""cv-header"">
                    <div class=""cv-name"">{EscapeHtml(profile.FullName)}</div>
                    <div class=""cv-title"">{EscapeHtml(profile.JobTitle)}</div>
                </header>
        
                <section class=""contact-grid"">
                    {RenderContactForTemplate3(profile)}
                </section>
        
                <div class=""cv-main"">
                    <!-- LEFT COLUMN -->
                    <div class=""left-column"">
                        {(string.IsNullOrWhiteSpace(summary) ? "" : $@"
                        <section class=""summary-section"">
                            <div class=""section-title"">Giới Thiệu</div>
                            <div class=""summary-content"">
                                {EscapeHtml(summary)}
                            </div>
                        </section>
                        ")}
                
                        {(cvData.Skills == null || !cvData.Skills.Any() ? "" : $@"
                        <section>
                            <div class=""section-title"">Kỹ Năng</div>
                            <div class=""skills-list"">
                                {RenderSkillsForTemplate3(cvData.Skills.Take(6).ToList())}
                            </div>
                        </section>
                        ")}
                
                        {(cvData.Educations == null || !cvData.Educations.Any() ? "" : $@"
                        <section>
                            <div class=""section-title"">Học Vấn</div>
                            <div id=""education-list"">
                                {RenderEducationForTemplate3(cvData.Educations.Take(2).ToList())}
                            </div>
                        </section>
                        ")}
                
                        {(cvData.Certificates == null || !cvData.Certificates.Any() ? "" : $@"
                        <section>
                            <div class=""section-title"">Chứng Chỉ</div>
                            <div id=""certifications-list"">
                                {RenderCertificatesForTemplate3(cvData.Certificates.Take(2).ToList())}
                            </div>
                        </section>
                        ")}
                    </div>
            
                    <!-- RIGHT COLUMN -->
                    <div class=""right-column"">
                        {(cvData.WorkExperiences == null || !cvData.WorkExperiences.Any() ? "" : $@"
                        <section>
                            <div class=""section-title"">Kinh Nghiệm Làm Việc</div>
                            <div id=""experience-list"">
                                {RenderWorkExperiencesForTemplate3(cvData.WorkExperiences.Take(3).ToList())}
                            </div>
                        </section>
                        ")}
                
                        {(cvData.Projects == null || !cvData.Projects.Any() ? "" : $@"
                        <section>
                            <div class=""section-title"">Dự Án</div>
                            <div id=""projects-list"">
                                {RenderProjectsForTemplate3(cvData.Projects.Take(2).ToList())}
                            </div>
                        </section>
                        ")}
                
                        <!-- NGÔN NGỮ & SỞ THÍCH -->
                        <div class=""row g-3"">
                            {(cvData.Languages == null || !cvData.Languages.Any() ? "" : $@"
                            <div class=""col-6"">
                                <div class=""section-title"">Ngôn Ngữ</div>
                                <div id=""languages-list"">
                                    {RenderLanguagesForTemplate3(cvData.Languages.Take(2).ToList())}
                                </div>
                            </div>
                            ")}
                            
                            {(cvData.Hobbies == null || !cvData.Hobbies.Any() ? "" : $@"
                            <div class=""col-6"">
                                <div class=""section-title"">Sở Thích</div>
                                <div class=""interests-list"" id=""interests-list"">
                                    {RenderInterestsForTemplate3(cvData.Hobbies.Take(3).ToList())}
                                </div>
                            </div>
                            ")}
                        </div>
                    </div>
                </div>
            </div>";
        }

        private string GetTemplate4Body(CvProfileRequest profile, CvSubmissionRequest cvData)
        {
            var summaryHtml = !string.IsNullOrWhiteSpace(profile.Summary) ? $@"
                <section class=""section"">
                    <h2 class=""section-title"">Giới thiệu</h2>
                    <div class=""section-content"">
                        <div>{EscapeHtml(profile.Summary)}</div>
                    </div>
                </section>" : "";

            var contactHtml = RenderContactSectionForTemplate4(profile);
            var skillsHtml = (cvData.Skills != null && cvData.Skills.Any()) ? $@"
                <section class=""section"">
                    <h2 class=""section-title"">Kỹ năng</h2>
                    <div class=""section-content"">
                        <div class=""skills-list"">
                            {RenderSkillsForTemplate4(cvData.Skills)}
                        </div>
                    </div>
                </section>" : "";

            var experiencesHtml = (cvData.WorkExperiences != null && cvData.WorkExperiences.Any()) ? $@"
                <section class=""section"">
                    <h2 class=""section-title"">Kinh nghiệm làm việc</h2>
                    <div class=""section-content"">
                        <div class=""experience-list"">
                            {RenderWorkExperiencesForTemplate4(cvData.WorkExperiences)}
                        </div>
                    </div>
                </section>" : "";

            var educationHtml = (cvData.Educations != null && cvData.Educations.Any()) ? $@"
                <section class=""section"">
                    <h2 class=""section-title"">Học vấn</h2>
                    <div class=""section-content"">
                        <div class=""experience-list"">
                            {RenderEducationForTemplate4(cvData.Educations)}
                        </div>
                    </div>
                </section>" : "";

            var certificatesHtml = (cvData.Certificates != null && cvData.Certificates.Any()) ? $@"
                <section class=""section"">
                    <h2 class=""section-title"">Chứng chỉ</h2>
                    <div class=""section-content"">
                        <div class=""experience-list"">
                            {RenderCertificatesForTemplate4(cvData.Certificates)}
                        </div>
                    </div>
                </section>" : "";

            return $@"
            <div id=""resume-container"">
                <header>
                    <h1>{EscapeHtml(profile.FullName)}</h1>
                    <h2>{EscapeHtml(profile.JobTitle)}</h2>
                </header>
        
                <main>
                    {summaryHtml}
                    {contactHtml}
                    {skillsHtml}
                    {experiencesHtml}
                    {educationHtml}
                    {certificatesHtml}
                </main>
            </div>";
        }

        private string GetTemplate5Body(CvProfileRequest profile, CvSubmissionRequest cvData)
        {
            return $@"
    <div class=""cv-container"">
        <!-- Header -->
        <header class=""cv-header"">
            <div class=""header-left"">
                <h1 class=""cv-name"">{EscapeHtml(profile.FullName)}</h1>
                <h2 class=""cv-position"">{EscapeHtml(profile.JobTitle)}</h2>
            </div>
            <div class=""contact-column"">
                <div class=""contact-list"">
                    {RenderContactForTemplate5(profile)}
                </div>
            </div>
        </header>

        <!-- Main Content -->
        <main class=""main-content"">
            <!-- Left Column -->
            <div class=""left-column"">
                <section class=""mb-4"">
                    <h2 class=""section-title"">Giới Thiệu</h2>
                    <p style=""line-height: 1.6; text-align: justify;"">{EscapeHtml(profile.Summary)}</p>
                </section>

                {RenderWorkExperiencesForTemplate5(cvData.WorkExperiences)}
                {RenderActivitiesForTemplate5(cvData.Projects)}
                {RenderProjectsForTemplate5(cvData.Projects)}
            </div>

            <!-- Right Column -->
            <div class=""right-column"">
                <!-- Certifications -->
                <section class=""mb-4"">
                    <h2 class=""section-title"">Chứng Chỉ</h2>
                    <div>
                        {RenderCertificatesForTemplate5(cvData.Certificates)}
                    </div>
                </section>

                <!-- Skills -->
                <section class=""mb-4"">
                    <h2 class=""section-title"">Kỹ Năng</h2>
                    <ul class=""skills-list"">
                        {RenderSkillsForTemplate5(cvData.Skills)}
                    </ul>
                </section>

                <!-- Education -->
                {RenderEducationForTemplate5(cvData.Educations)}
            </div>
        </main>
    </div>";
        }

        private string GetTemplate6Body(CvProfileRequest profile, CvSubmissionRequest cvData)
        {
            return $@"
    <div class=""cv-container"">
        <header class=""cv-header"">
            <div class=""header-main"">
                <h1 class=""cv-name"">{EscapeHtml(profile.FullName)}</h1>
                <h2 class=""cv-title"">{EscapeHtml(profile.JobTitle)}</h2>
            </div>
            <div class=""header-right"">
                <ul class=""contact-list"">
                    {RenderContactForTemplate6(profile)}
                </ul>
            </div>
        </header>

        <main>
            {(string.IsNullOrWhiteSpace(profile.Summary) ? "" : $@"
            <section class=""cv-section"">
                <h3 class=""section-title"">Giới thiệu</h3>
                <p>{EscapeHtml(profile.Summary)}</p>
            </section>
            ")}

            {(cvData.WorkExperiences == null || !cvData.WorkExperiences.Any() ? "" : $@"
            <section class=""cv-section"">
                <h3 class=""section-title"">Kinh nghiệm làm việc</h3>
                <div id=""experience-list"">
                    {RenderWorkExperiencesForTemplate6(cvData.WorkExperiences)}
                </div>
            </section>
            ")}

            {(cvData.Educations == null || !cvData.Educations.Any() ? "" : $@"
            <section class=""cv-section"">
                <h3 class=""section-title"">Học vấn</h3>
                <div id=""education-list"">
                    {RenderEducationForTemplate6(cvData.Educations)}
                </div>
            </section>
            ")}

            {(cvData.Projects == null || !cvData.Projects.Any() ? "" : $@"
            <section class=""cv-section"">
                <h3 class=""section-title"">Dự án</h3>
                <div id=""projects-list"">
                    {RenderProjectsForTemplate6(cvData.Projects)}
                </div>
            </section>
            ")}

            {(cvData.Skills == null || !cvData.Skills.Any() ? "" : $@"
            <section class=""cv-section"">
                <h3 class=""section-title"">Kỹ năng</h3>
                <div id=""skills-list"">
                    {RenderSkillsForTemplate6(cvData.Skills)}
                </div>
            </section>
            ")}

            {(cvData.Certificates == null || !cvData.Certificates.Any() ? "" : $@"
            <section class=""cv-section"">
                <h3 class=""section-title"">Chứng chỉ</h3>
                <div id=""certificates-list"">
                    {RenderCertificatesForTemplate6(cvData.Certificates)}
                </div>
            </section>
            ")}
        </main>
    </div>";
        }

        #endregion

        #region Shared Helper Methods

        // ========== CONTACT RENDERERS ==========
        private string RenderContactForTemplate1(CvProfileRequest profile)
        {
            var contacts = new List<string>();

            if (!string.IsNullOrEmpty(profile.ContactEmail))
            {
                contacts.Add($@"
        <li class=""contact-item"" data-type=""email"">
            <p class=""contact-value"">{EscapeHtml(profile.ContactEmail)}</p>
            <div class=""icon-wrapper"">
                <i class=""fas fa-envelope"" style=""color: white;""></i>
            </div>
        </li>");
            }

            if (!string.IsNullOrEmpty(profile.PhoneNumber))
            {
                contacts.Add($@"
        <li class=""contact-item"" data-type=""phone"">
            <p class=""contact-value"">{EscapeHtml(profile.PhoneNumber)}</p>
            <div class=""icon-wrapper"">
                <i class=""fas fa-phone"" style=""color: white;""></i>
            </div>
        </li>");
            }

            if (!string.IsNullOrEmpty(profile.Address))
            {
                contacts.Add($@"
        <li class=""contact-item"" data-type=""address"">
            <p class=""contact-value"">{EscapeHtml(profile.Address)}</p>
            <div class=""icon-wrapper"">
                <i class=""fas fa-map-marker-alt"" style=""color: white;""></i>
            </div>
        </li>");
            }

            if (profile.DateOfBirth.HasValue)
            {
                contacts.Add($@"
        <li class=""contact-item"" data-type=""birthday"">
            <p class=""contact-value"">{profile.DateOfBirth.Value:dd/MM/yyyy}</p>
            <div class=""icon-wrapper"">
                <i class=""fas fa-birthday-cake"" style=""color: white;""></i>
            </div>
        </li>");
            }

            if (!string.IsNullOrEmpty(profile.Gender))
            {
                contacts.Add($@"
        <li class=""contact-item"" data-type=""gender"">
            <p class=""contact-value"">{EscapeHtml(profile.Gender)}</p>
            <div class=""icon-wrapper"">
                <i class=""fas fa-venus-mars"" style=""color: white;""></i>
            </div>
        </li>");
            }

            return string.Join("", contacts);
        }

        private string RenderContactForTemplate2(CvProfileRequest profile)
        {
            var contacts = new List<string>();

            if (profile.DateOfBirth.HasValue)
            {
                contacts.Add($@"
            <li class=""contact-item"">
                <div class=""contact-icon""><i class=""fas fa-birthday-cake""></i></div>
                <div class=""contact-value"">{profile.DateOfBirth.Value:dd/MM/yyyy}</div>
            </li>");
            }

            if (!string.IsNullOrEmpty(profile.Address))
            {
                contacts.Add($@"
            <li class=""contact-item"">
                <div class=""contact-icon""><i class=""fas fa-map-marker-alt""></i></div>
                <div class=""contact-value"">{EscapeHtml(profile.Address)}</div>
            </li>");
            }

            if (!string.IsNullOrEmpty(profile.PhoneNumber))
            {
                contacts.Add($@"
            <li class=""contact-item"">
                <div class=""contact-icon""><i class=""fas fa-phone""></i></div>
                <div class=""contact-value"">{EscapeHtml(profile.PhoneNumber)}</div>
            </li>");
            }

            if (!string.IsNullOrEmpty(profile.ContactEmail))
            {
                contacts.Add($@"
            <li class=""contact-item"">
                <div class=""contact-icon""><i class=""fas fa-envelope""></i></div>
                <div class=""contact-value"">{EscapeHtml(profile.ContactEmail)}</div>
            </li>");
            }

            if (!string.IsNullOrEmpty(profile.Gender))
            {
                contacts.Add($@"
            <li class=""contact-item"">
                <div class=""contact-icon""><i class=""fas fa-venus-mars""></i></div>
                <div class=""contact-value"">{EscapeHtml(profile.Gender)}</div>
            </li>");
            }

            return string.Join("", contacts);
        }

        private string RenderContactForTemplate3(CvProfileRequest profile)
        {
            var contacts = new List<string>();

            if (!string.IsNullOrEmpty(profile.ContactEmail))
            {
                contacts.Add($@"
        <div class=""contact-item"">
            <i class=""fas fa-envelope contact-icon""></i>
            <span>{EscapeHtml(profile.ContactEmail)}</span>
        </div>");
            }

            if (!string.IsNullOrEmpty(profile.PhoneNumber))
            {
                contacts.Add($@"
        <div class=""contact-item"">
            <i class=""fas fa-phone contact-icon""></i>
            <span>{EscapeHtml(profile.PhoneNumber)}</span>
        </div>");
            }

            if (!string.IsNullOrEmpty(profile.Address))
            {
                contacts.Add($@"
        <div class=""contact-item"">
            <i class=""fas fa-map-marker-alt contact-icon""></i>
            <span>{EscapeHtml(profile.Address)}</span>
        </div>");
            }

            if (profile.DateOfBirth.HasValue)
            {
                contacts.Add($@"
        <div class=""contact-item"">
            <i class=""fas fa-birthday-cake contact-icon""></i>
            <span>{profile.DateOfBirth.Value:dd/MM/yyyy}</span>
        </div>");
            }

            if (!string.IsNullOrEmpty(profile.Gender))
            {
                contacts.Add($@"
        <div class=""contact-item"">
            <i class=""fas fa-venus-mars contact-icon""></i>
            <span>{EscapeHtml(profile.Gender)}</span>
        </div>");
            }

            return string.Join("", contacts);
        }

        private string RenderContactForTemplate4(CvProfileRequest profile)
        {
            var contacts = new List<string>();

            if (!string.IsNullOrEmpty(profile.ContactEmail))
            {
                contacts.Add($@"
            <div class=""contact-item"">
                <i class=""fas fa-envelope contact-icon""></i>
                <span>{EscapeHtml(profile.ContactEmail)}</span>
            </div>");
            }

            if (!string.IsNullOrEmpty(profile.PhoneNumber))
            {
                contacts.Add($@"
            <div class=""contact-item"">
                <i class=""fas fa-phone contact-icon""></i>
                <span>{EscapeHtml(profile.PhoneNumber)}</span>
            </div>");
            }

            if (!string.IsNullOrEmpty(profile.Address))
            {
                contacts.Add($@"
            <div class=""contact-item"">
                <i class=""fas fa-map-marker-alt contact-icon""></i>
                <span>{EscapeHtml(profile.Address)}</span>
            </div>");
            }

            if (profile.DateOfBirth.HasValue)
            {
                contacts.Add($@"
            <div class=""contact-item"">
                <i class=""fas fa-birthday-cake contact-icon""></i>
                <span>{profile.DateOfBirth.Value:dd/MM/yyyy}</span>
            </div>");
            }

            if (!string.IsNullOrEmpty(profile.Gender))
            {
                contacts.Add($@"
            <div class=""contact-item"">
                <i class=""fas fa-venus-mars contact-icon""></i>
                <span>{EscapeHtml(profile.Gender)}</span>
            </div>");
            }

            if (!string.IsNullOrEmpty(profile.Website))
            {
                contacts.Add($@"
            <div class=""contact-item"">
                <i class=""fas fa-globe contact-icon""></i>
                <span>{EscapeHtml(profile.Website)}</span>
            </div>");
            }
            return string.Join("", contacts);
        }

        private string RenderContactForTemplate5(CvProfileRequest profile)
        {
            var contacts = new List<string>();

            if (!string.IsNullOrEmpty(profile.ContactEmail))
            {
                contacts.Add($@"
            <div class=""contact-item"">
                <div class=""contact-icon""><i class=""fas fa-envelope""></i></div>
                <span>{EscapeHtml(profile.ContactEmail)}</span>
            </div>");
            }

            if (!string.IsNullOrEmpty(profile.PhoneNumber))
            {
                contacts.Add($@"
            <div class=""contact-item"">
                <div class=""contact-icon""><i class=""fas fa-phone""></i></div>
                <span>{EscapeHtml(profile.PhoneNumber)}</span>
            </div>");
            }

            if (!string.IsNullOrEmpty(profile.Address))
            {
                contacts.Add($@"
            <div class=""contact-item"">
                <div class=""contact-icon""><i class=""fas fa-map-marker-alt""></i></div>
                <span>{EscapeHtml(profile.Address)}</span>
            </div>");
            }

            if (profile.DateOfBirth.HasValue)
            {
                contacts.Add($@"
            <div class=""contact-item"">
                <div class=""contact-icon""><i class=""fas fa-calendar""></i></div>
                <span>{profile.DateOfBirth.Value:dd/MM/yyyy}</span>
            </div>");
            }

            return string.Join("", contacts);
        }

        private string RenderContactForTemplate6(CvProfileRequest profile)
        {
            var contacts = new List<string>();

            if (!string.IsNullOrEmpty(profile.ContactEmail))
            {
                contacts.Add($@"
        <li>
            <div class=""contact-value"">{EscapeHtml(profile.ContactEmail)}</div>
            <i class=""fas fa-envelope contact-icon""></i>
        </li>");
            }

            if (!string.IsNullOrEmpty(profile.PhoneNumber))
            {
                contacts.Add($@"
        <li>
            <div class=""contact-value"">{EscapeHtml(profile.PhoneNumber)}</div>
            <i class=""fas fa-phone contact-icon""></i>
        </li>");
            }

            if (!string.IsNullOrEmpty(profile.Address))
            {
                contacts.Add($@"
        <li>
            <div class=""contact-value"">{EscapeHtml(profile.Address)}</div>
            <i class=""fas fa-map-marker-alt contact-icon""></i>
        </li>");
            }

            if (profile.DateOfBirth.HasValue)
            {
                contacts.Add($@"
        <li>
            <div class=""contact-value"">{profile.DateOfBirth.Value:dd/MM/yyyy}</div>
            <i class=""fas fa-birthday-cake contact-icon""></i>
        </li>");
            }

            if (!string.IsNullOrEmpty(profile.Gender))
            {
                contacts.Add($@"
        <li>
            <div class=""contact-value"">{EscapeHtml(profile.Gender)}</div>
            <i class=""fas fa-venus-mars contact-icon""></i>
        </li>");
            }

            if (!string.IsNullOrEmpty(profile.Website))
            {
                contacts.Add($@"
        <li>
            <div class=""contact-value"">{EscapeHtml(profile.Website)}</div>
            <i class=""fas fa-globe contact-icon""></i>
        </li>");
            }
            return string.Join("", contacts);
        }

        private string RenderContactSectionForTemplate4(CvProfileRequest profile)
        {
            var contactItems = new List<string>();

            if (!string.IsNullOrEmpty(profile.ContactEmail))
            {
                contactItems.Add($@"
            <div class=""contact-item"">
                <i class=""fas fa-envelope""></i>
                <span>{EscapeHtml(profile.ContactEmail)}</span>
            </div>");
            }

            if (!string.IsNullOrEmpty(profile.PhoneNumber))
            {
                contactItems.Add($@"
            <div class=""contact-item"">
                <i class=""fas fa-phone""></i>
                <span>{EscapeHtml(profile.PhoneNumber)}</span>
            </div>");
            }

            if (!string.IsNullOrEmpty(profile.Address))
            {
                contactItems.Add($@"
            <div class=""contact-item"">
                <i class=""fas fa-map-marker-alt""></i>
                <span>{EscapeHtml(profile.Address)}</span>
            </div>");
            }

            if (profile.DateOfBirth.HasValue)
            {
                contactItems.Add($@"
            <div class=""contact-item"">
                <i class=""fas fa-birthday-cake""></i>
                <span>{profile.DateOfBirth.Value:dd/MM/yyyy}</span>
            </div>");
            }

            if (!string.IsNullOrEmpty(profile.Gender))
            {
                contactItems.Add($@"
            <div class=""contact-item"">
                <i class=""fas fa-venus-mars""></i>
                <span>{EscapeHtml(profile.Gender)}</span>
            </div>");
            }

            if (!string.IsNullOrEmpty(profile.Website))
            {
                contactItems.Add($@"
            <div class=""contact-item"">
                <i class=""fas fa-globe""></i>
                <span>{EscapeHtml(profile.Website)}</span>
            </div>");
            }

            if (contactItems.Any())
            {
                return $@"
        <section class=""section"">
            <h2 class=""section-title"">Liên hệ</h2>
            <div class=""section-content"">
                <div class=""contact-grid"">
                    {string.Join("", contactItems)}
                </div>
            </div>
        </section>";
            }

            return "";
        }

        // ========== WORK EXPERIENCE RENDERERS ==========
        private string RenderWorkExperiencesForTemplate1(List<CvWorkExperienceRequest> experiences)
        {
            if (experiences == null || !experiences.Any()) return "";

            var sb = new StringBuilder();
            foreach (var exp in experiences)
            {
                sb.AppendLine($@"
        <div class=""job-item"">
            <div class=""job-header"">
                <h4 class=""job-title"">{EscapeHtml(exp.JobTitle)}</h4>
            </div>
            <p class=""job-company"">{EscapeHtml(exp.CompanyName)}</p>
            <p class=""job-date"">{exp.StartDate:MM/yyyy} - {(exp.EndDate?.ToString("MM/yyyy") ?? "Present")}</p>
            <p class=""job-description"" style=""padding-left: 0; margin-left: 0; text-indent: 0;"">{EscapeHtml(exp.Description)}</p>
        </div>");
            }
            return sb.ToString();
        }

        private string RenderWorkExperiencesForTemplate2(List<CvWorkExperienceRequest> experiences)
        {
            if (experiences == null || !experiences.Any()) return "";

            var sb = new StringBuilder();
            foreach (var exp in experiences)
            {
                var descriptionLines = exp.Description?
                    .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                    .Where(line => !string.IsNullOrWhiteSpace(line))
                    .Select(line => line.Trim())
                    .ToList() ?? new List<string>();

                sb.AppendLine($@"
            <div class=""list-item"">
                <h4 class=""item-title"">{EscapeHtml(exp.JobTitle)}</h4>
                <p class=""item-subtitle"">{EscapeHtml(exp.CompanyName)} | {exp.StartDate:MM/yyyy} - {(exp.EndDate?.ToString("MM/yyyy") ?? "Hiện tại")}</p>
                {(descriptionLines.Any() ? $@"
                <ul class=""item-description"">
                    {string.Join("", descriptionLines.Select(line => $@"<li>{EscapeHtml(line.Replace("•", "").Trim())}</li>"))}
                </ul>" : "")}
            </div>");
            }
            return sb.ToString();
        }

        private string RenderWorkExperiencesForTemplate3(List<CvWorkExperienceRequest> experiences)
        {
            if (experiences == null || !experiences.Any())
                return @"<div class=""list-item"">Chưa có kinh nghiệm làm việc</div>";

            var sb = new StringBuilder();
            foreach (var exp in experiences)
            {
                sb.AppendLine($@"
        <div class=""list-item"">
            <div class=""item-header"">
                <div class=""item-title"">{EscapeHtml(exp.JobTitle)}</div>
                <div class=""item-date"">{exp.StartDate:MM/yyyy} - {(exp.EndDate?.ToString("MM/yyyy") ?? "Hiện tại")}</div>
            </div>
            <div class=""item-subtitle"">{EscapeHtml(exp.CompanyName)}</div>
            <div class=""item-description"">{EscapeHtml(exp.Description)}</div>
        </div>");
            }
            return sb.ToString();
        }

        private string RenderWorkExperiencesForTemplate4(List<CvWorkExperienceRequest> experiences)
        {
            if (experiences == null || !experiences.Any())
                return @"<div class=""experience-item""><p>Chưa có kinh nghiệm làm việc</p></div>";

            var sb = new StringBuilder();
            foreach (var exp in experiences)
            {
                var responsibilities = exp.Description?
                    .Split('\n')
                    .Where(line => !string.IsNullOrWhiteSpace(line))
                    .Select(line => line.Trim())
                    .ToList() ?? new List<string>();

                sb.AppendLine($@"
                <div class=""experience-item"">
                    <div class=""experience-item-header"">
                        <div>
                            <h3>{EscapeHtml(exp.JobTitle)}</h3>
                            <p>{EscapeHtml(exp.CompanyName)}</p>
                        </div>
                        <div class=""experience-item-date"">
                            {exp.StartDate:MM/yyyy} - {(exp.EndDate?.ToString("MM/yyyy") ?? "Hiện tại")}
                        </div>
                    </div>");

                if (responsibilities.Any())
                {
                    sb.AppendLine($@"<ul class=""responsibilities-list"">");
                    foreach (var responsibility in responsibilities)
                    {
                        sb.AppendLine($@"
                <li class=""responsibility-item"">
                    <div>{EscapeHtml(responsibility)}</div>
                </li>");
                    }
                    sb.AppendLine($@"</ul>");
                }

                sb.AppendLine($@"</div>");
            }
            return sb.ToString();
        }

        private string RenderWorkExperiencesForTemplate5(List<CvWorkExperienceRequest> experiences)
        {
            if (experiences == null || !experiences.Any())
                return $@"<section class=""mb-4""><h2 class=""section-title"">Kinh Nghiệm Làm Việc</h2><p>Chưa có thông tin</p></section>";

            var sb = new StringBuilder();
            sb.AppendLine($@"<section class=""mb-4""><h2 class=""section-title"">Kinh Nghiệm Làm Việc</h2>");

            foreach (var exp in experiences)
            {
                var descriptionLines = exp.Description?
                    .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                    .Where(line => !string.IsNullOrWhiteSpace(line))
                    .Select(line => line.Trim())
                    .ToList() ?? new List<string>();

                sb.AppendLine($@"
        <div class=""experience-item"">
            <div class=""item-header"">
                <div class=""item-title"">{EscapeHtml(exp.JobTitle)}</div>
                <div class=""item-company"">{EscapeHtml(exp.CompanyName)}</div>
                <div class=""item-period"">{exp.StartDate:MM/yyyy} - {(exp.EndDate?.ToString("MM/yyyy") ?? "Hiện tại")}</div>
            </div>");

                if (descriptionLines.Any())
                {
                    sb.AppendLine($@"<ul class=""item-description"">");
                    foreach (var line in descriptionLines)
                    {
                        sb.AppendLine($@"<li>{EscapeHtml(line)}</li>");
                    }
                    sb.AppendLine($@"</ul>");
                }

                sb.AppendLine($@"</div>");
            }

            sb.AppendLine("</section>");
            return sb.ToString();
        }

        private string RenderWorkExperiencesForTemplate6(List<CvWorkExperienceRequest> experiences)
        {
            if (experiences == null || !experiences.Any()) return "";

            var sb = new StringBuilder();
            foreach (var exp in experiences)
            {
                var descriptionLines = exp.Description?
                    .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                    .Where(line => !string.IsNullOrWhiteSpace(line))
                    .Select(line => line.Trim())
                    .ToList() ?? new List<string>();

                sb.AppendLine($@"
        <div class=""entry"">
            <div class=""entry-header"">
                <div>
                    <h4 class=""entry-title"">{EscapeHtml(exp.JobTitle)}</h4>
                    <p class=""entry-company"">{EscapeHtml(exp.CompanyName)}</p>
                </div>
                <div class=""entry-date"">{exp.StartDate:MM/yyyy} - {(exp.EndDate?.ToString("MM/yyyy") ?? "Hiện tại")}</div>
            </div>
            {(descriptionLines.Any() ? $@"
            <ul class=""entry-description"">
                {string.Join("", descriptionLines.Select(line => $@"<li>{EscapeHtml(line.Replace("•", "").Trim())}</li>"))}
            </ul>" : "")}
        </div>");
            }
            return sb.ToString();
        }

        // ========== EDUCATION RENDERERS ==========
        private string RenderEducationForTemplate1(List<CvEducationRequest> educations)
        {
            if (educations == null || !educations.Any()) return "";

            var sb = new StringBuilder();
            foreach (var edu in educations)
            {
                sb.AppendLine($@"
        <div class=""edu-item"">
            <p class=""edu-degree"">{EscapeHtml(edu.Major)}</p>
            <p class=""edu-institution"">{EscapeHtml(edu.SchoolName)}</p>
            <p class=""edu-date"">{edu.StartDate:yyyy} - {(edu.EndDate?.ToString("yyyy") ?? "Present")}</p>
        </div>");
            }
            return sb.ToString();
        }

        private string RenderEducationForTemplate2(List<CvEducationRequest> educations)
        {
            if (educations == null || !educations.Any()) return "";

            var sb = new StringBuilder();
            foreach (var edu in educations)
            {
                var descriptionLines = edu.Description?
                    .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                    .Where(line => !string.IsNullOrWhiteSpace(line))
                    .Select(line => line.Trim())
                    .ToList() ?? new List<string>();

                sb.AppendLine($@"
            <div class=""list-item"">
                <h4 class=""item-title"">{EscapeHtml(edu.Major)}</h4>
                <p class=""item-subtitle"">{EscapeHtml(edu.SchoolName)} | {edu.StartDate:yyyy} - {(edu.EndDate?.ToString("yyyy") ?? "Hiện tại")}</p>
                {(descriptionLines.Any() ? $@"
                <ul class=""item-description"">
                    {string.Join("", descriptionLines.Select(line => $@"<li>{EscapeHtml(line.Replace("•", "").Trim())}</li>"))}
                </ul>" : "")}
            </div>");
            }
            return sb.ToString();
        }

        private string RenderEducationForTemplate3(List<CvEducationRequest> educations)
        {
            if (educations == null || !educations.Any())
                return @"<div class=""list-item"">Chưa có thông tin học vấn</div>";

            var sb = new StringBuilder();
            foreach (var edu in educations)
            {
                sb.AppendLine($@"
        <div class=""list-item"">
            <div class=""item-header"">
                <div class=""item-title"">{EscapeHtml(edu.Major)}</div>
                <div class=""item-date"">{edu.StartDate:MM/yyyy} - {(edu.EndDate?.ToString("MM/yyyy") ?? "Hiện tại")}</div>
            </div>
            <div class=""item-subtitle"">{EscapeHtml(edu.SchoolName)}</div>
            <div class=""item-description"">{EscapeHtml(edu.Description)}</div>
        </div>");
            }
            return sb.ToString();
        }

        private string RenderEducationForTemplate4(List<CvEducationRequest> educations)
        {
            if (educations == null || !educations.Any())
                return @"<div class=""experience-item""><p>Chưa có thông tin học vấn</p></div>";

            var sb = new StringBuilder();
            foreach (var edu in educations)
            {
                sb.AppendLine($@"
        <div class=""experience-item"">
            <div class=""experience-item-header"">
                <div>
                    <h3>{EscapeHtml(edu.Major)}</h3>
                    <p>{EscapeHtml(edu.SchoolName)}</p>
                </div>
                <div class=""experience-item-date"">
                    {edu.StartDate:yyyy} - {(edu.EndDate?.ToString("yyyy") ?? "Hiện tại")}
                </div>
            </div>");

                if (!string.IsNullOrWhiteSpace(edu.Description))
                {
                    sb.AppendLine($@"
            <ul class=""responsibilities-list"">
                <li class=""responsibility-item"">
                    <div>{EscapeHtml(edu.Description)}</div>
                </li>
            </ul>");
                }

                sb.AppendLine($@"</div>");
            }
            return sb.ToString();
        }

        private string RenderEducationForTemplate5(List<CvEducationRequest> educations)
        {
            if (educations == null || !educations.Any())
                return $@"<section class=""mb-4""><h2 class=""section-title"">Học Vấn</h2><p>Chưa có thông tin</p></section>";

            var sb = new StringBuilder();
            sb.AppendLine($@"<section class=""mb-4""><h2 class=""section-title"">Học Vấn</h2>");

            foreach (var edu in educations)
            {
                var descriptionLines = edu.Description?
                    .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                    .Where(line => !string.IsNullOrWhiteSpace(line))
                    .Select(line => line.Trim())
                    .ToList() ?? new List<string>();

                sb.AppendLine($@"
        <div class=""experience-item"">
            <div class=""item-header"">
                <div class=""item-title"">{EscapeHtml(edu.Major)}</div>
                <div class=""item-company"">{EscapeHtml(edu.SchoolName)}</div>
                <div class=""item-period"">{edu.StartDate:yyyy} - {(edu.EndDate?.ToString("yyyy") ?? "Hiện tại")}</div>
            </div>");

                if (descriptionLines.Any())
                {
                    sb.AppendLine($@"<ul class=""item-description"">");
                    foreach (var line in descriptionLines)
                    {
                        sb.AppendLine($@"<li>{EscapeHtml(line)}</li>");
                    }
                    sb.AppendLine($@"</ul>");
                }

                sb.AppendLine($@"</div>");
            }

            sb.AppendLine("</section>");
            return sb.ToString();
        }

        private string RenderEducationForTemplate6(List<CvEducationRequest> educations)
        {
            if (educations == null || !educations.Any()) return "";

            var sb = new StringBuilder();
            foreach (var edu in educations)
            {
                var descriptionLines = edu.Description?
                    .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                    .Where(line => !string.IsNullOrWhiteSpace(line))
                    .Select(line => line.Trim())
                    .ToList() ?? new List<string>();

                sb.AppendLine($@"
        <div class=""entry"">
            <div class=""entry-header"">
                <div>
                    <h4 class=""entry-title"">{EscapeHtml(edu.Major)}</h4>
                    <p class=""entry-company"">{EscapeHtml(edu.SchoolName)}</p>
                </div>
                <div class=""entry-date"">{edu.StartDate:yyyy} - {(edu.EndDate?.ToString("yyyy") ?? "Hiện tại")}</div>
            </div>
            {(descriptionLines.Any() ? $@"
            <ul class=""entry-description"">
                {string.Join("", descriptionLines.Select(line => $@"<li>{EscapeHtml(line.Replace("•", "").Trim())}</li>"))}
            </ul>" : "")}
        </div>");
            }
            return sb.ToString();
        }

        // ========== SKILLS RENDERERS ==========
        private string RenderSkillsForTemplate1(List<CvSkillRequest> skills)
        {
            if (skills == null || !skills.Any()) return "";

            var sb = new StringBuilder();
            foreach (var skill in skills)
            {
                sb.AppendLine($@"
        <div class=""skill-item"">
            <p>{EscapeHtml(skill.SkillName)}</p>
        </div>");
            }
            return sb.ToString();
        }

        private string RenderSkillsForTemplate2(List<CvSkillRequest> skills)
        {
            if (skills == null || !skills.Any()) return "";

            var sb = new StringBuilder();
            foreach (var skill in skills)
            {
                sb.AppendLine($@"
            <div class=""list-item"">
                <h4 class=""item-title"">{EscapeHtml(skill.SkillName.Trim())}</h4>
            </div>");
            }
            return sb.ToString();
        }

        private string RenderSkillsForTemplate3(List<CvSkillRequest> skills)
        {
            if (skills == null || !skills.Any())
                return @"<div class=""skill-item"">Chưa có kỹ năng</div>";

            var sb = new StringBuilder();
            foreach (var skill in skills)
            {
                sb.AppendLine($@"
        <div class=""skill-item"">
            <span>{EscapeHtml(skill.SkillName)}</span>
        </div>");
            }
            return sb.ToString();
        }

        private string RenderSkillsForTemplate4(List<CvSkillRequest> skills)
        {
            if (skills == null || !skills.Any())
                return @"<div class=""skill-item""><span>Chưa có kỹ năng</span></div>";

            var sb = new StringBuilder();
            foreach (var skill in skills)
            {
                sb.AppendLine($@"
            <div class=""skill-item"">
                <span>{EscapeHtml(skill.SkillName)}</span>
            </div>");
            }
            return sb.ToString();
        }

        private string RenderSkillsForTemplate5(List<CvSkillRequest> skills)
        {
            if (skills == null || !skills.Any())
                return "<p>Chưa có kỹ năng</p>";

            var sb = new StringBuilder();
            foreach (var skill in skills)
            {
                sb.AppendLine($@"<li>{EscapeHtml(skill.SkillName)}</li>");
            }
            return sb.ToString();
        }

        private string RenderSkillsForTemplate6(List<CvSkillRequest> skills)
        {
            if (skills == null || !skills.Any()) return "";

            var sb = new StringBuilder();
            foreach (var skill in skills)
            {
                sb.AppendLine($@"
        <div class=""entry"">
            <div class=""entry-header"">
                <div>
                    <h4 class=""entry-title"">{EscapeHtml(skill.SkillName)}</h4>
                </div>
            </div>
        </div>");
            }
            return sb.ToString();
        }

        // ========== CERTIFICATES RENDERERS ==========
        private string RenderCertificatesForTemplate1(List<CvCertificateRequest> certificates)
        {
            if (certificates == null || !certificates.Any()) return "";

            var sb = new StringBuilder();
            foreach (var cert in certificates)
            {
                sb.AppendLine($@"
        <div class=""cert-item"">
            <p class=""cert-title"">{EscapeHtml(cert.CertificateName)}</p>
            <p class=""cert-date"">{cert.IssueDate:MM/yyyy}</p>
        </div>");
            }
            return sb.ToString();
        }

        private string RenderCertificatesForTemplate3(List<CvCertificateRequest> certificates)
        {
            if (certificates == null || !certificates.Any())
                return @"<div class=""list-item"">Chưa có chứng chỉ</div>";

            var sb = new StringBuilder();
            foreach (var cert in certificates)
            {
                sb.AppendLine($@"
        <div class=""list-item"">
            <div class=""item-header"">
                <div class=""item-title"">{EscapeHtml(cert.CertificateName)}</div>
                <div class=""item-date"">{cert.IssueDate:MM/yyyy}</div>
            </div>
            <div class=""item-description"">{EscapeHtml(cert.IssuingOrganization)}</div>
        </div>");
            }
            return sb.ToString();
        }

        private string RenderCertificatesForTemplate4(List<CvCertificateRequest> certificates)
        {
            if (certificates == null || !certificates.Any())
                return @"<div class=""experience-item""><p>Chưa có chứng chỉ</p></div>";

            var sb = new StringBuilder();
            foreach (var cert in certificates)
            {
                sb.AppendLine($@"
                <div class=""experience-item"">
                    <div class=""experience-item-header"">
                        <div>
                            <h3>{EscapeHtml(cert.CertificateName)}</h3>
                            <p>{EscapeHtml(cert.IssuingOrganization)}</p>
                        </div>
                        <div class=""experience-item-date"">
                            {cert.IssueDate:MM/yyyy}
                        </div>
                    </div>
                </div>");
            }
            return sb.ToString();
        }

        private string RenderCertificatesForTemplate5(List<CvCertificateRequest> certificates)
        {
            if (certificates == null || !certificates.Any())
                return "<p>Chưa có chứng chỉ</p>";

            var sb = new StringBuilder();
            foreach (var cert in certificates)
            {
                var details = cert.IssuingOrganization?.Split('\n') ?? new string[0];

                sb.AppendLine($@"
        <div class=""cert-item"">
            <div class=""item-header"">
                <div class=""item-title"">{EscapeHtml(cert.CertificateName)}</div>
                <div class=""item-period"">{cert.IssueDate:MM/yyyy}</div>
            </div>
            <div>
                {string.Join("", details.Where(line => !string.IsNullOrWhiteSpace(line)).Select(line => $@"<div>{EscapeHtml(line.Trim())}</div>"))}
            </div>
        </div>");
            }
            return sb.ToString();
        }

        private string RenderCertificatesForTemplate6(List<CvCertificateRequest> certificates)
        {
            if (certificates == null || !certificates.Any()) return "";

            var sb = new StringBuilder();
            foreach (var cert in certificates)
            {
                sb.AppendLine($@"
            <div class=""entry"">
                <div class=""entry-header"">
                    <div>
                        <h4 class=""entry-title"">{EscapeHtml(cert.CertificateName)}</h4>
                        <p class=""entry-company"">{EscapeHtml(cert.IssuingOrganization)}</p>
                    </div>
                    <div class=""entry-date"">{cert.IssueDate:MM/yyyy}</div>
                </div>
            </div>");
            }
            return sb.ToString();
        }

        // ========== PROJECTS RENDERERS ==========
        private string RenderProjectsForTemplate1(List<CvProjectRequest> projects)
        {
            if (projects == null || !projects.Any()) return "";

            var sb = new StringBuilder();
            foreach (var project in projects)
            {
                sb.AppendLine($@"
        <div class=""job-item"">
            <div class=""job-header"">
                <h4 class=""job-title"">{EscapeHtml(project.ProjectName)}</h4>
            </div>
            <p class=""job-company"">{EscapeHtml(project.ProjectUrl)}</p>
            <p class=""job-description"" style=""padding-left: 0; margin-left: 0; text-indent: 0;"">{EscapeHtml(project.Description)}</p>
        </div>");
            }
            return sb.ToString();
        }

        private string RenderProjectsForTemplate3(List<CvProjectRequest> projects)
        {
            if (projects == null || !projects.Any())
                return @"<div class=""list-item"">Chưa có dự án</div>";

            var sb = new StringBuilder();
            foreach (var project in projects)
            {
                sb.AppendLine($@"
        <div class=""list-item"">
            <div class=""item-header"">
                <div class=""item-title"">{EscapeHtml(project.ProjectName)}</div>
            </div>
            <div class=""item-description"">{EscapeHtml(project.Description)}</div>
        </div>");
            }
            return sb.ToString();
        }

        private string RenderProjectsForTemplate5(List<CvProjectRequest> projects)
        {
            var mainProjects = projects?.Where(p =>
                string.IsNullOrEmpty(p.ProjectName) ||
                !p.ProjectName.Contains("activities-")
            ).ToList();

            if (mainProjects == null || !mainProjects.Any())
                return $@"<section class=""mb-4""><h2 class=""section-title"">Dự Án</h2><p>Chưa có thông tin</p></section>";

            var sb = new StringBuilder();
            sb.AppendLine($@"<section class=""mb-4""><h2 class=""section-title"">Dự Án</h2>");

            foreach (var project in mainProjects)
            {
                var descriptionLines = project.Description?
                    .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                    .Where(line => !string.IsNullOrWhiteSpace(line))
                    .Select(line => line.Trim())
                    .ToList() ?? new List<string>();

                sb.AppendLine($@"
        <div class=""experience-item"">
            <div class=""item-header"">
                <div class=""item-title"">{EscapeHtml(project.ProjectName)}</div>
                <div class=""item-company"">{EscapeHtml(project.TechnologiesUsed ?? "")}</div>
                <div class=""item-period"">Thực hiện</div>
            </div>");

                if (descriptionLines.Any())
                {
                    sb.AppendLine($@"<ul class=""item-description"">");
                    foreach (var line in descriptionLines)
                    {
                        sb.AppendLine($@"<li>{EscapeHtml(line)}</li>");
                    }
                    sb.AppendLine($@"</ul>");
                }

                sb.AppendLine($@"</div>");
            }

            sb.AppendLine("</section>");
            return sb.ToString();
        }

        private string RenderProjectsForTemplate6(List<CvProjectRequest> projects)
        {
            if (projects == null || !projects.Any()) return "";

            var sb = new StringBuilder();
            foreach (var project in projects)
            {
                var descriptionLines = project.Description?
                    .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                    .Where(line => !string.IsNullOrWhiteSpace(line))
                    .Select(line => line.Trim())
                    .ToList() ?? new List<string>();

                sb.AppendLine($@"
        <div class=""entry"">
            <div class=""entry-header"">
                <div>
                    <h4 class=""entry-title"">{EscapeHtml(project.ProjectName)}</h4>
                    <p class=""entry-company"">{EscapeHtml(project.TechnologiesUsed ?? "")}</p>
                </div>
                <div class=""entry-date"">{EscapeHtml(project.ProjectUrl ?? "")}</div>
            </div>
            {(descriptionLines.Any() ? $@"
            <ul class=""entry-description"">
                {string.Join("", descriptionLines.Select(line => $@"<li>{EscapeHtml(line.Replace("•", "").Trim())}</li>"))}
            </ul>" : "")}
        </div>");
            }
            return sb.ToString();
        }

        // ========== REFERENCES RENDERERS ==========
        private string RenderReferencesForTemplate1(List<CvReferenceRequest> references)
        {
            if (references == null || !references.Any()) return "";

            var sb = new StringBuilder();
            foreach (var reference in references)
            {
                var contactInfo = EscapeHtml(reference.ContactInfo ?? "").Trim();

                sb.AppendLine($@"
            <div class=""edu-item"">
                <p class=""edu-degree"">{EscapeHtml(reference.FullName)}</p>
                <p class=""edu-institution"">{EscapeHtml(reference.JobTitle)} - {EscapeHtml(reference.Company)}</p>
                <p class=""edu-date"">{contactInfo}</p>
            </div>");
            }
            return sb.ToString();
        }

        // ========== INTERESTS/HOBBIES RENDERERS ==========
        private string RenderInterestsForTemplate1(List<CvHobbyRequest> hobbies)
        {
            if (hobbies == null || !hobbies.Any()) return "";

            var sb = new StringBuilder();
            foreach (var hobby in hobbies)
            {
                sb.AppendLine($@"
        <div>
            <span class=""interest-item"">{EscapeHtml(hobby.HobbyName)}</span>
        </div>");
            }
            return sb.ToString();
        }

        private string RenderInterestsForTemplate3(List<CvHobbyRequest> hobbies)
        {
            if (hobbies == null || !hobbies.Any())
                return @"<span class=""interest-tag"">Chưa có sở thích</span>";

            var sb = new StringBuilder();
            foreach (var hobby in hobbies)
            {
                sb.AppendLine($@"
        <span class=""interest-tag"">{EscapeHtml(hobby.HobbyName)}</span>");
            }
            return sb.ToString();
        }

        // ========== LANGUAGES RENDERERS ==========
        private string RenderLanguagesForTemplate3(List<CvLanguageRequest> languages)
        {
            if (languages == null || !languages.Any())
                return @"<div class=""skill-item"">Chưa có ngôn ngữ</div>";

            var sb = new StringBuilder();
            foreach (var lang in languages)
            {
                sb.AppendLine($@"
        <div class=""skill-item"">
            <span>{EscapeHtml(lang.LanguageName)}</span>
        </div>");
            }
            return sb.ToString();
        }

        // ========== ACTIVITIES RENDERERS ==========
        private string RenderActivitiesForTemplate5(List<CvProjectRequest> projects)
        {
            var activities = projects?.Where(p =>
                !string.IsNullOrEmpty(p.ProjectName) &&
                p.ProjectName.Contains("activities-")
            ).ToList();

            if (activities == null || !activities.Any())
                return $@"<section class=""mb-4""><h2 class=""section-title"">Hoạt Động</h2><p>Chưa có thông tin</p></section>";

            var sb = new StringBuilder();
            sb.AppendLine($@"<section class=""mb-4""><h2 class=""section-title"">Hoạt Động</h2>");

            foreach (var activity in activities)
            {
                var displayName = activity.ProjectName?.Replace("activities-", "") ?? "";
                var descriptionLines = activity.Description?
                    .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                    .Where(line => !string.IsNullOrWhiteSpace(line))
                    .Select(line => line.Trim())
                    .ToList() ?? new List<string>();

                sb.AppendLine($@"
        <div class=""experience-item"">
            <div class=""item-header"">
                <div class=""item-title"">{EscapeHtml(activity.Role ?? displayName)}</div>
                <div class=""item-company"">{EscapeHtml(activity.TechnologiesUsed ?? "")}</div>
                <div class=""item-period"">Tham gia</div>
            </div>");

                if (descriptionLines.Any())
                {
                    sb.AppendLine($@"<ul class=""item-description"">");
                    foreach (var line in descriptionLines)
                    {
                        sb.AppendLine($@"<li>{EscapeHtml(line)}</li>");
                    }
                    sb.AppendLine($@"</ul>");
                }

                sb.AppendLine($@"</div>");
            }

            sb.AppendLine("</section>");
            return sb.ToString();
        }

        #endregion

        #region Utility Methods
        private string FormatDateRange(DateTime startDate, DateTime? endDate)
        {
            var end = endDate?.ToString("MM/yyyy") ?? "hiện tại";
            return $"{startDate:MM/yyyy} - {end}";
        }

        private string EscapeHtml(string input)
        {
            if (string.IsNullOrEmpty(input)) return "";
            return System.Net.WebUtility.HtmlEncode(input);
        }

        private async Task<string> ConvertHtmlToPdf(string htmlContent, string fullName)
        {
            await new BrowserFetcher().DownloadAsync();

            using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = true,
                Args = new[] { "--no-sandbox" }
            });

            using var page = await browser.NewPageAsync();
            await page.SetContentAsync(htmlContent);
            await page.WaitForNetworkIdleAsync(new WaitForNetworkIdleOptions { IdleTime = 500, Timeout = 5000 });

            var pdfsDir = Path.Combine(_environment.WebRootPath ?? "wwwroot", "pdfs");
            if (!Directory.Exists(pdfsDir))
                Directory.CreateDirectory(pdfsDir);

            var fileName = $"CV_{SanitizeFileName(fullName)}_{DateTime.Now:yyyyMMddHHmmss}";
            var pdfPath = Path.Combine(pdfsDir, $"{fileName}.pdf");

            await page.PdfAsync(pdfPath, new PdfOptions
            {
                Format = PaperFormat.A4,
                PrintBackground = true,
                MarginOptions = new MarginOptions
                {
                    Top = "0.5cm",
                    Right = "0.5cm",
                    Bottom = "0.5cm",
                    Left = "0.5cm"
                }
            });

            return pdfPath;
        }

        private string SanitizeFileName(string fileName)
        {
            return Path.GetInvalidFileNameChars()
                .Aggregate(fileName ?? "unknown", (current, c) => current.Replace(c, '_'));
        }
        #endregion
    }
}