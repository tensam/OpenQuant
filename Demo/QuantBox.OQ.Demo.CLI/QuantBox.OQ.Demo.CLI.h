// QunatBox.OQ.Demo.CLI.h

#pragma once

using namespace System;

namespace QuantBoxOQDemoCLI {

	public class UnmanagedClass
	{
	public:
		virtual void OnBarOpen(OpenQuant::API::Bar^ bar);
		virtual void OnBar(OpenQuant::API::Bar^ bar);
	protected:
	private:
	};

	public ref class ManagedClass: public OpenQuant::API::Strategy
	{
	public:
		virtual void OnStrategyStart() override;
		virtual void OnBarOpen(OpenQuant::API::Bar^ bar) override;
		virtual void OnBar(OpenQuant::API::Bar^ bar) override;

	public:
		ManagedClass():m_Impl( new UnmanagedClass ){}
		~ManagedClass(){delete m_Impl;}
	protected:
		!ManagedClass(){delete m_Impl;}
	private:
		UnmanagedClass * m_Impl;
	};
}

/*
�������OpenQuant.API.dll

�����dll,���Ƶ�OpenQuant\BinĿ¼��

��OpenQuant�Ĳ����ļ���code.cs������д
using OpenQuant.API;
using QunatBoxOQDemoCLI;

public class MyStrategy : ManagedClass
{
}

��ReflectorҲ������UnmanageClass������еľ�������ˡ�
*/
