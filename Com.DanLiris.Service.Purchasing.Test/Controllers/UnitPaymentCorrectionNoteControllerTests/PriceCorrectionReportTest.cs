﻿using AutoMapper;
using Com.DanLiris.Service.Purchasing.Lib.Interfaces;
using Com.DanLiris.Service.Purchasing.Lib.Models.UnitPaymentCorrectionNoteModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.UnitPaymentOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.Services;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.IntegrationViewModel;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.UnitPaymentCorrectionNoteViewModel;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.UnitPaymentOrderViewModel;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.UnitPaymentOrderDataUtils;
using Com.DanLiris.Service.Purchasing.Test.Helpers;
using Com.DanLiris.Service.Purchasing.WebApi.Controllers.v1.UnitPaymentCorrectionNoteController;
using Com.Moonlay.NetCore.Lib.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Com.DanLiris.Service.Purchasing.Test.Controllers.UnitPaymentCorrectionNoteControllerTests
{
    public class PriceCorrectionReportTest
    {
        private UnitPaymentPriceCorrectionNoteReportViewModel ViewModel
        {
            get
            {
                return new UnitPaymentPriceCorrectionNoteReportViewModel
                {
                    supplier = "SupplierName",
                    correctionType = "Harga Satuan",
                    correctionDate = new DateTimeOffset(),
                    prNo = "pr123",
                    upcNo = "upc123",
                    upoNo = "upo123",
                    epoNo = "123",
                    quantity = 1,
                    productName = "ProductName",
                    productCode = "ProductCode",
                    uom = "UomUnit",
                    vatTaxCorrectionDate = null,
                    vatTaxCorrectionNo = "kl",
                    user = "test",
                    LastModifiedUtc = new DateTime(),
                    pricePerDealUnitAfter=2000,
                    priceTotalAfter=1700,
                };
            }
        }

        private ServiceValidationExeption GetServiceValidationExeption()
        {
            Mock<IServiceProvider> serviceProvider = new Mock<IServiceProvider>();
            List<ValidationResult> validationResults = new List<ValidationResult>();
            System.ComponentModel.DataAnnotations.ValidationContext validationContext = new System.ComponentModel.DataAnnotations.ValidationContext(this.ViewModel, serviceProvider.Object, null);
            return new ServiceValidationExeption(validationContext, validationResults);
        }

        private Mock<IServiceProvider> GetServiceProvider()
        {
            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider
                .Setup(x => x.GetService(typeof(IdentityService)))
                .Returns(new IdentityService() { Token = "Token", Username = "Test" });

            serviceProvider
                .Setup(x => x.GetService(typeof(IHttpClientService)))
                .Returns(new HttpClientTestService());

            return serviceProvider;
        }

        private UnitPaymentPriceCorrectionNoteReportController GetController(Mock<IUnitPaymentPriceCorrectionNoteFacade> facadeM, Mock<IValidateService> validateM, Mock<IMapper> mapper)
        {
            var user = new Mock<ClaimsPrincipal>();
            var claims = new Claim[]
            {
                new Claim("username", "unittestusername")
            };
            user.Setup(u => u.Claims).Returns(claims);

            var servicePMock = GetServiceProvider();
            servicePMock
                .Setup(x => x.GetService(typeof(IValidateService)))
                .Returns(validateM.Object);

            UnitPaymentPriceCorrectionNoteReportController controller = new UnitPaymentPriceCorrectionNoteReportController(servicePMock.Object, mapper.Object, facadeM.Object)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = new DefaultHttpContext()
                    {
                        User = user.Object
                    }
                }
            };
            controller.ControllerContext.HttpContext.Request.Headers["Authorization"] = "Bearer unittesttoken";
            controller.ControllerContext.HttpContext.Request.Path = new PathString("/v1/unit-test");
            controller.ControllerContext.HttpContext.Request.Headers["x-timezone-offset"] = "7";

            return controller;
        }

        protected int GetStatusCode(IActionResult response)
        {
            return (int)response.GetType().GetProperty("StatusCode").GetValue(response, null);
        }

        [Fact]
        public void Should_Success_Get_Report_Data()
        {
            var mockFacade = new Mock<IUnitPaymentPriceCorrectionNoteFacade>();
            mockFacade.Setup(x => x.GetReport(null,null, It.IsAny<int>(), It.IsAny<int>(), "{}",It.IsAny<int>()))
                .Returns(Tuple.Create( new List<UnitPaymentPriceCorrectionNoteReportViewModel> { ViewModel },25));

            var mockMapper = new Mock<IMapper>();
            mockMapper.Setup(x => x.Map<List<UnitPaymentPriceCorrectionNoteReportViewModel>>(It.IsAny<List<UnitPaymentCorrectionNote>>()))
                .Returns(new List<UnitPaymentPriceCorrectionNoteReportViewModel> { ViewModel });

            var user = new Mock<ClaimsPrincipal>();
            var claims = new Claim[]
            {
                new Claim("username", "unittestusername")
            };
            user.Setup(u => u.Claims).Returns(claims);
            UnitPaymentPriceCorrectionNoteReportController controller = new UnitPaymentPriceCorrectionNoteReportController(GetServiceProvider().Object, mockMapper.Object, mockFacade.Object);
            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
                {
                    User = user.Object
                }
            };
            controller.ControllerContext.HttpContext.Request.Headers["x-timezone-offset"] = "0";
            var response = controller.GetReport(null,null,1, 25, "{}");
            Assert.Equal((int)HttpStatusCode.OK, GetStatusCode(response));
        }

        [Fact]
        public void Should_Error_Get_Report_Xls_Data()
        {
            var mockFacade = new Mock<IUnitPaymentPriceCorrectionNoteFacade>();
            mockFacade.Setup(x => x.GetReport(null, null, It.IsAny<int>(), It.IsAny<int>(), "{}", It.IsAny<int>()))
                .Returns(Tuple.Create(new List<UnitPaymentPriceCorrectionNoteReportViewModel> { ViewModel }, 25));

            var mockMapper = new Mock<IMapper>();
            mockMapper.Setup(x => x.Map<List<UnitPaymentPriceCorrectionNoteReportViewModel>>(It.IsAny<List<UnitPaymentCorrectionNote>>()))
                .Returns(new List<UnitPaymentPriceCorrectionNoteReportViewModel> { ViewModel });

            var user = new Mock<ClaimsPrincipal>();
            var claims = new Claim[]
            {
                new Claim("username", "unittestusername")
            };
            user.Setup(u => u.Claims).Returns(claims);
            UnitPaymentPriceCorrectionNoteReportController controller = new UnitPaymentPriceCorrectionNoteReportController(GetServiceProvider().Object, mockMapper.Object, mockFacade.Object);
            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
                {
                    User = user.Object
                }
            };
            controller.ControllerContext.HttpContext.Request.Headers["x-timezone-offset"] = "0";
            var response = controller.GetXls(null, null);
            Assert.Equal((int)HttpStatusCode.InternalServerError, GetStatusCode(response));
        }

        [Fact]
        public void Should_Success_Get_Report_Xls_Data()
        {
            var mockFacade = new Mock<IUnitPaymentPriceCorrectionNoteFacade>();
            mockFacade.Setup(x => x.GenerateExcel(null, null, It.IsAny<int>()))
                .Returns(new MemoryStream());

            var mockMapper = new Mock<IMapper>();
            mockMapper.Setup(x => x.Map<List<UnitPaymentPriceCorrectionNoteReportViewModel>>(It.IsAny<List<UnitPaymentCorrectionNote>>()))
                .Returns(new List<UnitPaymentPriceCorrectionNoteReportViewModel> { ViewModel });

            var user = new Mock<ClaimsPrincipal>();
            var claims = new Claim[]
            {
                new Claim("username", "unittestusername")
            };
            user.Setup(u => u.Claims).Returns(claims);
            UnitPaymentPriceCorrectionNoteReportController controller = new UnitPaymentPriceCorrectionNoteReportController(GetServiceProvider().Object, mockMapper.Object, mockFacade.Object);
            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
                {
                    User = user.Object
                }
            };
            controller.ControllerContext.HttpContext.Request.Headers["x-timezone-offset"] = "0";
            var response = controller.GetXls(null, null);
            Assert.Equal(null, response.GetType().GetProperty("FileStream"));
        }
    }
}
