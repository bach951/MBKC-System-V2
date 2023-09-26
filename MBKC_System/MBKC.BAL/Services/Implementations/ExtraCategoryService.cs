﻿using AutoMapper;
using MBKC.BAL.Services.Interfaces;
using MBKC.DAL.Infrastructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

    {
    public class ExtraCategoryService : IExtraCategoryService
    {
        private UnitOfWork _unitOfWork;
        private IMapper _mapper;
        public ExtraCategoryService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            this._unitOfWork = (UnitOfWork)unitOfWork;
            this._mapper = mapper;
        }
    }
}
